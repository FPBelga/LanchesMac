using LanchesMac.Areas.Services;
using LanchesMac.Context;
using LanchesMac.Models;
using LanchesMac.Repositories;
using LanchesMac.Repositories.Interfaces;
using LanchesMac.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.CodeAnalysis.Options;
using Microsoft.EntityFrameworkCore;
using ReflectionIT.Mvc.Paging;

namespace LanchesMac;
public class Startup
{
    public Startup(IConfiguration configuration)
    {
        Configuration = configuration;
    }

    public IConfiguration Configuration { get; }

    public void ConfigureServices(IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

        //Registrando os serviços do Identity para Controle de Usuário
        services.AddIdentity<IdentityUser, IdentityRole>()
             .AddEntityFrameworkStores<AppDbContext>()
             .AddDefaultTokenProviders();

       // services.ConfigureApplicationCookie(options => options.AccessDeniedPath = "Home/AccesDenied");
        services.Configure<ConfigurationImagens>(Configuration.GetSection("ConfigurationPastaImagens"));

        //services.Configure<IdentityOptions>(options =>
        //{
        //    // Default Password settings.
        //    options.Password.RequireDigit = false;
        //    options.Password.RequireLowercase = false;
        //    options.Password.RequireNonAlphanumeric = false;
        //    options.Password.RequireUppercase = false;
        //    options.Password.RequiredLength = 3;
        //    options.Password.RequiredUniqueChars = 1;
        //});

        //Criando a injenção de dependência no container nativo para criar a instância do objeto da interface
        services.AddTransient<ILancheRepository, LancheRepository>();
        services.AddTransient<ICategoriaRepository, CategoriaRepository>();
        services.AddTransient<IPedidoRepository, PedidoRepository>();
        services.AddScoped<ISeedUserRoleInitial, SeedUserRoleInitial>();
        services.AddScoped<RelatorioVendasService>();

        //Incluindo a Politica de acesso informando o perfil "Admin" como necessário
        services.AddAuthorization(options =>
        {
            options.AddPolicy("Admin",
                politica =>
                {
                    politica.RequireRole("Admin");
                });
        });

        //Habilitando os recursos do HTTPContext
        services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        //Cria o carrinho a cada requisição
        services.AddScoped(sp => CarrinhoCompra.GetCarrinho(sp));

        services.AddControllersWithViews();

        services.AddPaging(options =>
        {
            options.ViewName = "Bootstrap4";
            options.PageParameterName = "pageindex";
        });
        //Habilitando o cache da sessão para otimizar a sessão
        services.AddMemoryCache();
        //services.AddDistributedMemoryCache();

        services.AddSession();
        //{
        //    options.IdleTimeout = TimeSpan.FromSeconds(10);
        //    options.Cookie.HttpOnly = true;
        //    options.Cookie.IsEssential = true;
        //});
    }

    public void Configure(IApplicationBuilder app,
        IWebHostEnvironment env, ISeedUserRoleInitial seedUserRoleInitial)
    {
        if (env.IsDevelopment())
        {
            app.UseDeveloperExceptionPage();
        }
        else
        {
            app.UseExceptionHandler("/Home/Error");
            app.UseHsts();
        }
        app.UseHttpsRedirection();

        app.UseStaticFiles();
        app.UseRouting();

        //Cria os usuários e atribui ao perfil
        seedUserRoleInitial.SeedRoles();
        //cria os usuários e atribui ao perfil
        seedUserRoleInitial.SeedUsers();

        //Ativando o middleware da sessão
        app.UseSession();
        app.UseSession();
        
        //Ativando o middleware de autenticação
        app.UseAuthentication();

        app.UseAuthorization();


        app.UseEndpoints(endpoints =>
        {
            endpoints.MapControllerRoute(
             name: "areas",
             pattern: "{area:exists}/{controller=Admin}/{action=Index}/{id?}");

            endpoints.MapControllerRoute(
               name: "categoriaFiltro",
               pattern: "Lanche/{action}/{categoria?}",
               defaults: new { Controller = "Lanche", action = "List" });

            endpoints.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");
        });
    }
}