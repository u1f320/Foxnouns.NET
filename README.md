# Foxnouns.NET

Rewrite of pronouns.cc's codebase in C#, using SvelteKit for the frontend.
Still very work-in-progress, but a large portion of the backend is functional.

## License

    Copyright (C) 2024  sam <u1f320>

    This program is free software: you can redistribute it and/or modify
    it under the terms of the GNU Affero General Public License as published
    by the Free Software Foundation, either version 3 of the License, or
    (at your option) any later version.

    This program is distributed in the hope that it will be useful,
    but WITHOUT ANY WARRANTY; without even the implied warranty of
    MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
    GNU Affero General Public License for more details.

    You should have received a copy of the GNU Affero General Public License
    along with this program.  If not, see <https://www.gnu.org/licenses/>.

## Acknowledgements

Codebases I've used for inspiration/figuring things out:

- [Iceshrimp.NET](https://iceshrimp.dev/iceshrimp/Iceshrimp.NET)
- [PluralKit](https://github.com/PluralKit/PluralKit)

Code taken entirely or almost entirely from external sources:

- The functions in the `AddSids` migration,
  taken from [PluralKit](https://github.com/PluralKit/PluralKit/blob/32a6e97342acc3b35e6f9e7b4dd169e21d888770/PluralKit.Core/Database/Functions/functions.sql)
- `Foxnouns.Backend/Database/prune-designer-cs-files.sh`,
  taken from [Iceshrimp.NET](https://iceshrimp.dev/iceshrimp/Iceshrimp.NET/src/commit/7c93dcf79dda54fc1a4ea9772e3f80874e6bcefb/Iceshrimp.Backend/Core/Database/prune-designer-cs-files.sh)
