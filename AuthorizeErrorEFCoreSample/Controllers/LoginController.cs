using AuthorizeErrorEFCoreSample.Models;
using DBLib.DAL;
using DBLib.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Linq;

namespace AuthorizeErrorEFCoreSample.Controllers
{
    public class LoginController : Controller
    {
        private readonly NorthwindContext db;
        public IConfiguration Configuration { get; }

        public LoginController(NorthwindContext _dbcontext, IConfiguration configuration)
        {
            this.db = _dbcontext;
            this.Configuration = configuration;
        }

        [Route("/login")]
        [HttpGet]
        [AllowAnonymous]
        public IActionResult Login()
        {
            ViewData["VerifyFailed"] = "N";

            return View();
        }

        [Route("/login")]
        [HttpPost]
        [AllowAnonymous]
        public IActionResult Login([FromForm] LoginViewModel FormData)
        {
            ViewData["VerifyFailed"] = "N";

            // 後端驗證資料欄位，確認每個欄位資料都驗證通過
            if (ModelState.IsValid)
            {
                bool TempCheckResult = false;
                int TempUid = 0;

                if (FormData.account != null && FormData.pwd != null)
                {
                    using (db)
                    {
                        var GetUserRecord = (from udata in db.Set<Employees>()
                                             where udata.FirstName == FormData.account.ToString() && udata.Pwd == FormData.pwd.ToString()
                                             select new
                                             {
                                                 udata.EmployeeId
                                             }).FirstOrDefault()
                                            ;

                        TempCheckResult = GetUserRecord != null ? true : false;
                        TempUid = GetUserRecord != null ? GetUserRecord.EmployeeId : 0;
                    }
                }

                if (TempCheckResult)
                {
                    Uri GenUrl = new Uri($"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/employees/index");

                    // 建立登入驗證Session
                    HttpContext.Session.SetString("UID", TempUid.ToString());

                    // 轉向網址至管理後台首頁
                    HttpContext.Response.Redirect(GenUrl.ToString());
                }
                else
                {
                    ViewData["VerifyFailed"] = "Y";
                }
            }

            return View();
        }
    }
}