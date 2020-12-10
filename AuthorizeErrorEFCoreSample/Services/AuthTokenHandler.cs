using DBLib.DAL;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace AuthorizeErrorEFCoreSample.Services
{
    public class AuthTokenHandler : AuthenticationHandler<AuthTokenOptions>
    {
        private readonly NorthwindContext db;

        public AuthTokenHandler(
        IOptionsMonitor<AuthTokenOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        NorthwindContext _DBContext
        )
        : base(options, logger, encoder, clock)
        {
            this.db = _DBContext;
        }

        /// <summary>
        /// 處理Token驗證，透過Service注入來驗證有設置[Authorize] Action
        /// </summary>
        /// <returns></returns>
        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            string CurrentUIDSessionToken = Context.Session.GetString("UID");

            // 判斷是否有UID Session，如果未有就代表未登入
            if (!string.IsNullOrEmpty(CurrentUIDSessionToken))
            {
                bool CheckResult = false;

                // 錯誤版本
                // 因為在管道生命週期中，每一次進入Action之前必定會跑驗證的Middleware
                // 然後會跑到這一段執行結束後，會把整個DBContext物件給釋放掉，導致跑道後方Action如果有用到DBContext時
                // 會發生以下錯誤，因為Action中DBContext物件已經實質上變空物件
                // ObjectDisposedException: Cannot access a disposed context instance. 
                //using (db)
                //{
                //    int GetDbRecord = db.Employees
                //        .Where(data => data.EmployeeId == int.Parse(CurrentUIDSessionToken))
                //        .Count();

                //    CheckResult = GetDbRecord != 0 ? true : false;
                //}

                // 正確版本
                int GetDbRecord = db.Employees
                        .Where(data => data.EmployeeId == int.Parse(CurrentUIDSessionToken))
                        .Count();
                CheckResult = GetDbRecord != 0 ? true : false;

                if (CheckResult)
                {
                    // 建立Claims
                    var claims = new ClaimsPrincipal(new ClaimsIdentity[]{
                    new ClaimsIdentity(
                        new Claim[] {
                            new Claim(ClaimsIdentity.DefaultNameClaimType, CurrentUIDSessionToken) // 放置UID
                        },
                        "AuthToken" // 必須要加入authenticationType，否則會被作為未登入
                    ) });

                    // 回傳驗證成功訊息並返回Claims
                    return AuthenticateResult.Success(new AuthenticationTicket(claims, "AuthToken"));
                }

                return AuthenticateResult.Fail("Authorize Verify Failed.");
            }
            else
            {
                // 網站剛啟動時候會跑到這一段一次，因為在Service有注入，所以初始化啟動必定會跑到這一次。
                return AuthenticateResult.NoResult();
            }
        }
    }
}