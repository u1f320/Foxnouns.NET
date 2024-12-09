// Copyright (C) 2023-present sam/u1f320 (vulpine.solutions)
//
// This program is free software: you can redistribute it and/or modify
// it under the terms of the GNU Affero General Public License as published
// by the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU Affero General Public License for more details.
//
// You should have received a copy of the GNU Affero General Public License
// along with this program.  If not, see <https://www.gnu.org/licenses/>.
using System.IO.Compression;
using System.Net;
using Coravel.Invocable;
using Foxnouns.Backend.Database;
using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Services;
using Foxnouns.Backend.Utils;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using NodaTime;
using NodaTime.Text;

namespace Foxnouns.Backend.Jobs;

public class CreateDataExportInvocable(
    DatabaseContext db,
    IClock clock,
    UserRendererService userRenderer,
    MemberRendererService memberRenderer,
    ObjectStorageService objectStorageService,
    ISnowflakeGenerator snowflakeGenerator,
    ILogger logger
) : IInvocable, IInvocableWithPayload<CreateDataExportPayload>
{
    private static readonly HttpClient Client = new();
    private readonly ILogger _logger = logger.ForContext<CreateDataExportInvocable>();
    public required CreateDataExportPayload Payload { get; set; }

    public async Task Invoke()
    {
        try
        {
            await InvokeAsync();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Error generating data export for user {UserId}", Payload.UserId);
        }
    }

    private async Task InvokeAsync()
    {
        User? user = await db
            .Users.Include(u => u.AuthMethods)
            .Include(u => u.Flags)
            .Include(u => u.ProfileFlags)
            .AsSplitQuery()
            .FirstOrDefaultAsync(u => u.Id == Payload.UserId);
        if (user == null)
        {
            _logger.Warning(
                "Received create data export request for user {UserId} but no such user exists, deleted or otherwise. Ignoring request",
                Payload.UserId
            );
            return;
        }

        _logger.Information("Generating data export for user {UserId}", user.Id);

        await using var stream = new MemoryStream();
        using var zip = new ZipArchive(stream, ZipArchiveMode.Create, true);
        zip.Comment =
            $"This archive for {user.Username} ({user.Id}) was generated at  {InstantPattern.General.Format(clock.GetCurrentInstant())}";

        // Write the user's info and avatar
        WriteJson(
            zip,
            "user.json",
            await userRenderer.RenderUserInnerAsync(user, true, ["*"], false, true)
        );
        await WriteS3Object(zip, "user-avatar.webp", userRenderer.AvatarUrlFor(user));

        foreach (PrideFlag? flag in user.Flags)
            await WritePrideFlag(zip, flag);

        List<Member> members = await db
            .Members.Include(m => m.User)
            .Include(m => m.ProfileFlags)
            .Where(m => m.UserId == user.Id)
            .ToListAsync();
        foreach (Member? member in members)
            await WriteMember(zip, member);

        // We want to dispose the ZipArchive on an error, but we need to dispose it manually to upload to object storage.
        // Calling Dispose() multiple times is fine for this class, though.
        // ReSharper disable once DisposeOnUsingVariable
        zip.Dispose();
        stream.Seek(0, SeekOrigin.Begin);

        // Upload the file!
        string filename = AuthUtils.RandomToken().Replace('+', '-').Replace('/', '_');
        await objectStorageService.PutObjectAsync(
            ExportPath(user.Id, filename),
            stream,
            "application/zip"
        );

        db.Add(
            new DataExport
            {
                Id = snowflakeGenerator.GenerateSnowflake(),
                UserId = user.Id,
                Filename = filename,
            }
        );
        await db.SaveChangesAsync();
    }

    private async Task WritePrideFlag(ZipArchive zip, PrideFlag flag)
    {
        if (flag.Hash == null)
        {
            _logger.Debug("Flag {FlagId} has a null hash, ignoring it", flag.Id);
            return;
        }

        _logger.Debug("Writing flag {FlagId}", flag.Id);

        var flagData = $"""
            {flag.Name}
            ----
            {flag.Description ?? "<no description>"}
            """;

        try
        {
            await WriteS3Object(zip, $"flag-{flag.Id}/flag.webp", userRenderer.ImageUrlFor(flag));
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Could not write image for flag {FlagId}", flag.Id);
            return;
        }

        ZipArchiveEntry entry = zip.CreateEntry($"flag-{flag.Id}/flag.txt");
        await using Stream stream = entry.Open();
        await using var writer = new StreamWriter(stream);
        await writer.WriteAsync(flagData);
    }

    private async Task WriteMember(ZipArchive zip, Member member)
    {
        _logger.Debug("Writing member {MemberId}", member.Id);

        WriteJson(
            zip,
            $"members/{member.Name} ({member.Id}).json",
            memberRenderer.RenderMember(member)
        );

        try
        {
            await WriteS3Object(
                zip,
                $"members/{member.Name} ({member.Id}).webp",
                memberRenderer.AvatarUrlFor(member)
            );
        }
        catch (Exception e)
        {
            _logger.Warning(e, "Error writing avatar for member {MemberId}", member.Id);
        }
    }

    private void WriteJson(ZipArchive zip, string filename, object data)
    {
        string json = JsonConvert.SerializeObject(data, Formatting.Indented);

        _logger.Debug(
            "Writing file {Filename} to archive with size {Length}",
            filename,
            json.Length
        );

        ZipArchiveEntry entry = zip.CreateEntry(filename);
        using Stream stream = entry.Open();
        using var writer = new StreamWriter(stream);
        writer.Write(json);
    }

    private async Task WriteS3Object(ZipArchive zip, string filename, string? s3Path)
    {
        if (s3Path == null)
            return;

        HttpResponseMessage resp = await Client.GetAsync(s3Path);
        if (resp.StatusCode != HttpStatusCode.OK)
        {
            _logger.Warning("S3 path {S3Path} returned a non-200 status, not saving file", s3Path);
            return;
        }

        await using Stream respStream = await resp.Content.ReadAsStreamAsync();

        _logger.Debug(
            "Writing file {Filename} to archive with size {Length}",
            filename,
            respStream.Length
        );

        ZipArchiveEntry entry = zip.CreateEntry(filename);
        await using Stream entryStream = entry.Open();

        respStream.Seek(0, SeekOrigin.Begin);
        await respStream.CopyToAsync(entryStream);
    }

    private static string ExportPath(Snowflake userId, string b64) =>
        $"data-exports/{userId}/{b64}.zip";
}
