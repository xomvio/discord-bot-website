using Bitguard.Services;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Reflection;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using AspNetCoreRateLimit;
using Microsoft.AspNetCore.HttpOverrides;
using Bitguard.DiscordRazor;

var builder = WebApplication.CreateBuilder(args);

/*builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenAnyIP(443, listenopts =>
    {
		listenopts.UseHttps("/var/sites/certificate.pfx", "daniasu");
    });
	options.ListenAnyIP(80);
});*/


builder.Services.AddControllersWithViews();

builder.Services.AddHttpContextAccessor();
//builder.Services.AddEnyimMemcached();

builder.Services.Configure<CookiePolicyOptions>(options =>
{
	options.Secure = CookieSecurePolicy.Always;
});


	//CONFIGURE FORWARDED HEADERS	!!!
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
	options.KnownProxies.Add(IPAddress.Parse("10.0.0.100"));
})

builder.Services.AddAuthentication(options =>
{
	options.DefaultChallengeScheme = CookieAuthenticationDefaults.AuthenticationScheme;
	options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
	options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
})
.AddCookie()
.AddJwtBearer(options =>        //Cookie i�in bizim tokenimiz
{
	options.TokenValidationParameters = new TokenValidationParameters
	{
		ValidateIssuer = true,
		ValidateAudience = false,
		ValidateIssuerSigningKey = true,

		ValidIssuer = builder.Configuration.GetValue<string>("JWT:Issuer"),
		ValidAudience = builder.Configuration.GetValue<string>("JWT:Audience"),
		IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(builder.Configuration.GetValue<string>("JWT:EncryptionKey")))
	};
})
.AddOAuth("Discord", options => //Discord api
{
	options.AuthorizationEndpoint = "https://discord.com/api/oauth2/authorize";
	options.Scope.Add("identify");
	options.Scope.Add("guilds");
	options.CallbackPath = "/auth/oauthcallback";
	options.ClientId = builder.Configuration.GetValue<string>("Discord:ClientId");
	options.ClientSecret = builder.Configuration.GetValue<string>("Discord:ClientSecret");
	options.TokenEndpoint = "https://discord.com/api/oauth2/token";
	options.UserInformationEndpoint = "https://discord.com/api/users/@me";


	options.AccessDeniedPath = "/DiscordAuthFailed"; //Kullan�c�n�n yetki vermemesi durumunda bu sayfa. Sayfa i�eri�i olu�turulacak.

	options.Events = new Microsoft.AspNetCore.Authentication.OAuth.OAuthEvents
	{
		OnCreatingTicket = async context =>
		{
			context.Identity.AddClaim(new Claim("token", context.AccessToken));

			context.RunClaimActions();

		}
	};
});


//ratelimiting services starts
builder.Services.AddOptions();
builder.Services.AddMemoryCache();
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimiting"));
builder.Services.Configure<IpRateLimitOptions>(builder.Configuration.GetSection("IpRateLimitPolicies"));
builder.Services.AddSingleton<IIpPolicyStore, MemoryCacheIpPolicyStore>();
builder.Services.AddSingleton<IRateLimitCounterStore, MemoryCacheRateLimitCounterStore>();
builder.Services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
builder.Services.AddSingleton<IProcessingStrategy, AsyncKeyLockProcessingStrategy>();
//ratelimiting ends


#region Localizer
builder.Services.AddSingleton<LanguageService>();
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddMvc().AddViewLocalization().AddDataAnnotationsLocalization(options =>
	options.DataAnnotationLocalizerProvider = (type, factory) =>
	{
		var assemblyName = new AssemblyName(typeof(SharedResource).GetTypeInfo().Assembly.FullName);
		return factory.Create(nameof(SharedResource), assemblyName.Name);
	});
builder.Services.Configure<RequestLocalizationOptions>(options =>
{
	var supportedCultures = new List<CultureInfo>
	{
		new CultureInfo("en-US"),
		new CultureInfo("tr-TR"),
	};
	options.DefaultRequestCulture = new RequestCulture(culture: "en-US", uiCulture: "en-US");
	options.SupportedCultures = supportedCultures;
	options.SupportedUICultures = supportedCultures;
	options.RequestCultureProviders.Insert(0, new QueryStringRequestCultureProvider());
});
#endregion


var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
	Directory.SetCurrentDirectory(AppDomain.CurrentDomain.BaseDirectory);   //For kestrel
	app.UseExceptionHandler("/Home/Error");
	app.UseStatusCodePagesWithRedirects("/Home/Error?status={0}");
	// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
	app.UseHsts();
}

//app.UseEnyimMemcached();
//app.UseHttpsRedirection();	//cloudflare ile sonsuz redirect d�ng�s�
app.UseStaticFiles();

app.UseRequestLocalization(app.Services.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value);


app.UseForwardedHeaders(new ForwardedHeadersOptions //ip alabilmek i�in
{
	ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
});


app.UseCookiePolicy(new CookiePolicyOptions()
{
	MinimumSameSitePolicy = SameSiteMode.Lax    //Disk�rtten cookie kabul etmesi i�in.
});

app.UseRouting();

app.UseAuthorization();
app.UseAuthentication();

	//FORWARDED HEADER ICIN KNKKKK !!!
app.MapGet("/", () => "10.0.0.100");

app.MapControllerRoute(
	name: "default",
	pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
