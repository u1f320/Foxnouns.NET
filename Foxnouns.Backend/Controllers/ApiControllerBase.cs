using Foxnouns.Backend.Database.Models;
using Foxnouns.Backend.Middleware;
using Microsoft.AspNetCore.Mvc;

namespace Foxnouns.Backend.Controllers;

[ApiController]
[Authenticate]
public class ApiControllerBase : ControllerBase
{
    internal Token? Token => HttpContext.GetToken();
    internal new User? User => HttpContext.GetUser();
}