using ElectronicsStore.Models;
using ElectronicsStore.Models.DTO;
using ElectronicsStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using sib_api_v3_sdk.Client;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography.Xml;
using System.Threading.Tasks;

namespace ElectronicsStore.Controllers
{
    [Route("/Admin/[controller]/{action=Register}/{id?}")]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> userManager;
        private readonly SignInManager<ApplicationUser> signInManager;
        private readonly RoleManager<IdentityRole> roleManager;
        private readonly IConfiguration _configuration;

        public AccountController(UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager,
            RoleManager<IdentityRole> roleManager, IConfiguration configuration)
        {
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.roleManager = roleManager;
            _configuration = configuration;
        }
        public IActionResult Register()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Register(RegisterDto registerDto)
        {
            if (!ModelState.IsValid)
            {
                return View(registerDto);
            }
            var user = new ApplicationUser
            {
                FirstName = registerDto.FirstName,
                LastName = registerDto.LastName,
                Email = registerDto.Email,
                UserName = registerDto.Email,
                PhoneNumber = registerDto.PhoneNumber,
                Address = registerDto.Address,
                CreateAt = DateTime.Now
            };
            var result = await userManager.CreateAsync(user, registerDto.Password);
            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(user, "client");
                await signInManager.SignInAsync(user, false);
                return RedirectToAction("index", "Home");
            }
            foreach (var error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(registerDto);

        }
        public async Task<IActionResult> Logout()
        {
            if (signInManager.IsSignedIn(User))
            {
                await signInManager.SignOutAsync();
            }
            return RedirectToAction("index", "home");
        }
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return View(loginDto);
            }

            var user = await userManager.FindByEmailAsync(loginDto.Email);
            if (user == null)
            {
                ViewBag.ErrorMessage = "Invalid login attempt.";
                return View(loginDto);
            }

            var result = await signInManager.PasswordSignInAsync(
                user.UserName,
                loginDto.Password,
                loginDto.Remberme,
                lockoutOnFailure: false
            );

            if (result.Succeeded)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ErrorMessage = "Invalid login attempt.";
            return View(loginDto);
        }
        [Authorize]
        public async Task<IActionResult> Profile()
        {
            var appUser = await userManager.GetUserAsync(User);
            if (appUser == null)
            {
                return RedirectToAction("index", "home");
            }
            var profileDto = new EditProfile()
            {
                FirstName = appUser.FirstName,
                LastName = appUser.LastName,
                Email = appUser.Email,
                Address = appUser.Address,
                PhoneNumber = appUser.PhoneNumber
            };
            return View(profileDto);
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> Profile(EditProfile editProfileDto)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = "Please Fill All Required Field With valid Data";
                return View(editProfileDto);
            }
            var appUser = await userManager.GetUserAsync(User);

            appUser.FirstName = editProfileDto.FirstName;
            appUser.LastName = editProfileDto.LastName;
            appUser.PhoneNumber = editProfileDto.PhoneNumber;
            appUser.Email = editProfileDto.Email;
            appUser.Address = editProfileDto.Address;

            var result = await userManager.UpdateAsync(appUser);

            if (!result.Succeeded)
            {
                ViewBag.ErrorMessage = "Unable to Update Profile " + result.Errors.First().Description;
            }
            else
            {
                ViewBag.secuessMessage = "Update Data Successfull";
            }

            return View(editProfileDto);
        }
        [Authorize]
        public IActionResult ChangePassword()
        {
            return View();
        }
        [Authorize]
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordDto changePasswordDto)
        {
            if (!ModelState.IsValid)
            {
                return View();
            }
            var appUser = await userManager.GetUserAsync(User);
            var result = await userManager.ChangePasswordAsync(appUser, changePasswordDto.CurrentPassword, changePasswordDto.NewPassword);
            if (!result.Succeeded)
            {
                ViewBag.ErrorMessage = "Error " + result.Errors.First().Description;
                return View();
            }
            else
            {
                ViewBag.secuessMessage = "Change Password Successfully";
            }
            return View();
        }
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword([Required, EmailAddress] string email)
        {
            ViewBag.Email = email;

            if (!ModelState.IsValid)
            {
                ViewBag.ErrorMessage = ModelState["Email"].Errors
                    .First().ErrorMessage ?? "Invalid Email Address";
                return View();
            }

            var user = await userManager.FindByEmailAsync(email);

            if (user != null)
            {
                var token = await userManager.GeneratePasswordResetTokenAsync(user);
                var resultUrl = Url.Action(
                    action: "ResetPassword",
                    controller: "Account",
                    values: new { userId = user.Id, code = token },
                    protocol: Request.Scheme
                ) ?? "Url Error";

                string senderName = _configuration["BrevoSetting:SenderName"] ?? "";
                string senderEmail = _configuration["BrevoSetting:EmailSender"] ?? "";
                string userName = $"{user.FirstName} {user.LastName}";
                string subject = "Reset Password";
                string htmlContent = $@"
                <p>Dear {userName},</p>
                <p>We received a request to reset your password. Please click the link below to reset your password:</p>
                <p><a href=""{resultUrl}"">Reset Password</a></p>
                <p>If you did not request this, please ignore this email.</p>
                <p>Thank you!</p>";

                EmailSender.SendEmail(
                    apiKey: _configuration["BrevoSetting:Api_key"],
                    senderName: senderName,
                    senderEmail: senderEmail,
                    recipientName: userName,
                    recipientEmail: email,
                    subject: subject,
                    htmlContent: htmlContent
                );
            }

            ViewBag.SuccessMessage = "Please check your email and click the reset password link.";
            return View();
        }
        public IActionResult AccessDenied()
        {
            return RedirectToAction("index", "home");
        }
    }
}
