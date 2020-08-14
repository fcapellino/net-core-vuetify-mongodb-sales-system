namespace BasicSalesSystem.Web.Controllers
{
    using System;
    using System.IdentityModel.Tokens.Jwt;
    using System.Linq;
    using System.Security.Claims;
    using System.Text;
    using System.Threading.Tasks;
    using BasicSalesSystem.Domain.Entities;
    using BasicSalesSystem.Web.Custom;
    using BasicSalesSystem.Web.Custom.Enumerations;
    using BasicSalesSystem.Web.Custom.Resources;
    using BasicSalesSystem.Web.Dependencies.EmailService;
    using BasicSalesSystem.Web.Dependencies.MongoDbService;
    using BasicSalesSystem.Web.Requests.User;
    using Microsoft.AspNetCore.Authorization;
    using Microsoft.AspNetCore.Identity;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.Extensions.Caching.Memory;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.IdentityModel.Tokens;
    using MongoDB.Driver;
    using MongoDB.Driver.Linq;
    using ISR = Microsoft.AspNetCore.Identity;

    //[Authorize]
    [AllowAnonymous]
    [Route("Api/[controller]/[action]")]
    public class UsersController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _memoryCache;
        private readonly EmailService _emailService;
        private readonly MongoDbService _mongoDbService;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public UsersController(IServiceProvider serviceProvider)
        {
            _configuration = serviceProvider.GetService<IConfiguration>();
            _memoryCache = serviceProvider.GetService<IMemoryCache>();
            _emailService = serviceProvider.GetService<EmailService>();
            _mongoDbService = serviceProvider.GetService<MongoDbService>();
            _signInManager = serviceProvider.GetService<SignInManager<ApplicationUser>>();
            _userManager = serviceProvider.GetService<UserManager<ApplicationUser>>();
        }

        [HttpGet]
        //[Authorization(UserRoles.Administrator, UserRoles.Regular)]
        public async Task<IActionResult> GetUsersList(GetUsersListRequest request)
        {
            var usersQuery = _mongoDbService.GetCollection<ApplicationUser>(Collections.Users).AsQueryable();
            var rolesQuery = _mongoDbService.GetCollection<ApplicationRole>(Collections.Roles).AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.RoleId))
            {
                usersQuery = usersQuery.Where(x => x.Roles.Contains(request.RoleId));
            }

            if (!string.IsNullOrWhiteSpace(request.SearchQuery))
            {
                long.TryParse(request.SearchQuery, out long number);

                usersQuery = usersQuery.Where(x =>
                    x.FullName.ToLower().Contains(request.SearchQuery.ToLower().Trim()) ||
                    x.Address.ToLower().Contains(request.SearchQuery.ToLower().Trim()) ||
                    x.DocumentNumber == number ||
                    x.Email.ToLower().Contains(request.SearchQuery.ToLower().Trim()));
            }

            var totalItemCount = await usersQuery.CountAsync();
            var users = await usersQuery
                .ApplyOrdering(request.SortBy, request.SortDesc)
                .ApplyPaging(request.Page, request.PageSize)
                .ToListAsync();

            var roles = await rolesQuery
                .ToListAsync();

            var resources = new PagedListResource()
            {
                TotalItemCount = totalItemCount,
                ItemsList = users
                    .Select(item =>
                    {
                        var roleId = item.Roles.First();
                        var role = roles.FirstOrDefault(r => r.Id.ToString().Equals(roleId));
                        return new
                        {
                            item.Id,
                            item.FullName,
                            item.Address,
                            item.DocumentType,
                            item.DocumentNumber,
                            item.Email,
                            item.Enabled,
                            Role = new
                            {
                                role.Id,
                                Name = role.Name.ToUpper()
                            }
                        };
                    })
                    .ToList()
            };

            return new SuccessResult(resources);
        }

        [HttpGet]
        //[Authorization(UserRoles.Administrator, UserRoles.Regular)]
        public async Task<IActionResult> GetUsersRoleList()
        {
            var query = _mongoDbService.GetCollection<ApplicationRole>(Collections.Roles).AsQueryable();
            var items = await query
                .ApplyOrdering(nameof(ApplicationRole.Name), descending: false)
                .ToListAsync();

            var resources = new ListResource()
            {
                ItemsList = items
                            .Select(item => new
                            {
                                item.Id,
                                Name = item.Name.ToUpper(),
                                item.Description,
                                item.Active
                            })
                            .ToList()
            };

            return new SuccessResult(resources);
        }

        [HttpPost]
        //[Authorization(UserRoles.Administrator)]
        public async Task<IActionResult> CreateUser([FromBody] CreateNewUserRequest request)
        {
            var filter = Builders<ApplicationUser>.Filter.Where(x => x.Email.Equals(request.Email));
            var userAlreadyExists = await _mongoDbService.GetCollection<ApplicationUser>(Collections.Users)
                .Find(filter).AnyAsync();

            if (userAlreadyExists)
            {
                throw new CustomException("The entered email is already in use.");
            }

            var newUser = new ApplicationUser()
            {
                FullName = request.FullName.Trim(),
                Address = request.Address.Trim(),
                DocumentType = request.DocumentType.ToUpper().Trim(),
                DocumentNumber = request.DocumentNumber,
                Email = request.Email.Trim().Normalize().ToLowerInvariant(),
                UserName = request.Email.Trim().Normalize().ToLowerInvariant(),
                Enabled = true
            };

            var password = Guid.NewGuid().ToString("n").Substring(0, 8).ToLower();
            var createResult = await _userManager.CreateAsync(newUser, password);
            var addToRoleResult = await _userManager.AddToRoleAsync(newUser, request.Role.ToLower());

            if (!(createResult.Succeeded && addToRoleResult.Succeeded))
            {
                throw new InvalidOperationException();
            }

            string appBaseUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.PathBase}";
            var emailBody = $"Your account has been successfully created. Password: <b>[ {password} ].</b> To access the site click on the following <a target='_blank' href='{appBaseUrl}'>link.</a>";
            await _emailService.SendAsync(newUser.Email, "Basic Sales System", emailBody);

            return new SuccessResult();
        }

        [HttpPost]
        //[Authorization(UserRoles.Administrator)]
        public async Task<IActionResult> EnableOrDisableUser(String id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new CustomException("Invalid user specified.");
            }

            user.Enabled = !user.Enabled;
            await _userManager.UpdateAsync(user);
            return new SuccessResult();
        }

        [HttpPost]
        //[Authorization(UserRoles.Administrator)]
        public async Task<IActionResult> DeleteUser(String id)
        {
            var user = await _userManager.FindByIdAsync(id.ToString());
            if (user == null)
            {
                throw new CustomException("The user you are trying to delete does not exist.");
            }

            await _userManager.DeleteAsync(user);
            return new SuccessResult();
        }

        [HttpPost]
        //[Authorization(UserRoles.Administrator, UserRoles.Regular)]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
        {
            var currentUserId = HttpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var currentUser = await _userManager.FindByIdAsync(currentUserId);
            var result = await _userManager.ChangePasswordAsync(currentUser, request.OldPassword.Trim(), request.NewPassword.Trim());

            if (!result.Succeeded)
            {
                throw new CustomException("You have entered an invalid password.");
            }

            return new SuccessResult();
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> GenerateJSONWebToken([FromBody] GenerateJSONWebTokenRequest request)
        {
            var applicationUser = await _userManager.FindByEmailAsync(request.Email.Trim().Normalize().ToLowerInvariant());
            var result = new ISR.SignInResult();

            if (applicationUser != null)
            {
                result = await _signInManager.CheckPasswordSignInAsync(applicationUser, request.Password.Trim(), lockoutOnFailure: false);
            }

            if (!result.Succeeded)
            {
                throw new CustomException("The username or password is incorrect.");
            }

            var userRoles = await _userManager.GetRolesAsync(applicationUser);
            var claimsIdentity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.NameIdentifier, applicationUser?.Id.ToString()) });
            claimsIdentity.AddClaims(userRoles.Select(role => new Claim(ClaimTypes.Role, role)));

            var tokenHandler = new JwtSecurityTokenHandler();
            var secret = _configuration.GetSection("JwtSettings:Secret").Value;
            var expirationTime = _configuration.GetSection("JwtSettings:ExpirationTime").Get<int>();
            var tokenDescriptor = new SecurityTokenDescriptor()
            {
                Expires = DateTime.UtcNow.AddMinutes(expirationTime),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secret)), SecurityAlgorithms.HmacSha256Signature),
                Subject = claimsIdentity
            };

            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            var token = tokenHandler.WriteToken(securityToken);

            return new SuccessResult(new { Token = token });
        }

        [HttpPost]
        [AllowAnonymous]
        public async Task<IActionResult> RefreshJSONWebToken([FromBody] RefreshJSONWebTokenRequest request)
        {
            //                return await _cache.GetOrCreate($"puntosentrega:{pais}:{marca}".ToLower(), async entry =>
            //        {
            //    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromDays(1);
            //    var puntosDeEntregaUrl = _configuration.GetSection($"RequestURLs:{pais}:PuntosDeEntrega").Value +
            //        $"?sinCoordenadas=false&idMarca={marca.NullIfEmpty()}";

            //    var puntosDeEntregaResponse = await _httpClientWrapper.GetStringAsync(puntosDeEntregaUrl);
            //    return new SuccessResult(JsonConvert.DeserializeObject<dynamic>(puntosDeEntregaResponse));

            return new SuccessResult();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            _memoryCache?.Dispose();
            _emailService?.Dispose();
            _mongoDbService?.Dispose();
            _userManager?.Dispose();
        }
    }
}
