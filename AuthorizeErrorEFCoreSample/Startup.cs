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

            services.AddDistributedMemoryCache();//�ϥΤ��s�O����ӰO��
            services.AddSession(options =>
            {
                //options.Cookie.Name = "Authon"; //���A��Cookie�R�W �i�bF12�ݨ�
                options.IdleTimeout = TimeSpan.FromSeconds(10);//���m10�����n�X
            });

            // �[�J����
            // �[�J����
            services
                .AddAuthentication("AuthToken")
                .AddScheme<AuthTokenOptions, AuthTokenHandler>("AuthToken", null);

            // �[�J���v
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

            // �o�̦b�����Ҧp�G���v�O���Ī��|�Q�^��401�A���^��401�ɡA�|�ɦV�^����w�����h
            // �ݭn�`�N�b�ˬd���ҳB�z�y�{���O�_����DBContext�����񱼡A�|�ɭP�᭱Action���p�G���ե�DBContext�ɷ|�o��Null�Ѧҿ��~
            app.Use(async (context, next) =>
            {
                await next();

                if (context.Response.StatusCode == 401)
                {
                    context.Response.Redirect(Configuration.GetValue<string>("VerifyFailedRedirectUrl"));
                }
            });

            app.UseAuthentication(); // ����
            app.UseAuthorization(); // ���v (Controller�BAction�~��[�W [Authorize] �ݩ�)

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}