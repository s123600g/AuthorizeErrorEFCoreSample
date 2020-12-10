using AuthorizeErrorEFCoreSample.Services;
using DBLib.DAL;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;

namespace AuthorizeErrorEFCoreSample
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews().AddRazorRuntimeCompilation();
            services.AddDbContext<NorthwindContext>(config =>
            {
                config.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
            });

            services.AddDistributedMemoryCache();//使用內存記憶體來記錄
            services.AddSession(options =>
            {
                //options.Cookie.Name = "Authon"; //為你的Cookie命名 可在F12看見
                options.IdleTimeout = TimeSpan.FromSeconds(10);//閒置10分鐘登出
            });

            // 加入驗證
            // 加入驗證
            services
                .AddAuthentication("AuthToken")
                .AddScheme<AuthTokenOptions, AuthTokenHandler>("AuthToken", null);

            // 加入授權
            services.AddAuthorization();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }
            app.UseStaticFiles();
            app.UseSession();
            app.UseRouting();

            // 這裡在做驗證如果授權是失效的會被回傳401，當有回傳401時，會導向回到指定頁面去
            // 需要注意在檢查驗證處理流程中是否有把DBContext給釋放掉，會導致後面Action內如果有調用DBContext時會發生Null參考錯誤
            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == 401)
                {
                    context.Response.Redirect(Configuration.GetValue<string>("VerifyFailedRedirectUrl"));
                }
            });

            app.UseAuthentication(); // 驗證
            app.UseAuthorization(); // 授權 (Controller、Action才能加上 [Authorize] 屬性)

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}