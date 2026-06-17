using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SIMBG.API.Datos;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// debe agregarse este cloque para permitir CORS, es decir, que el API pueda ser consumida desde cualquier origen
builder.Services.AddCors(opciones =>
{
    opciones.AddPolicy("PermitirTodo", politica =>
    {
        politica.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});


// Add services to the container.
//se modifica el archivo appsettings.jason y se le dan parámetros que ahora se leen en esta línea
var cadena = builder.Configuration.GetConnectionString("ConexionRaspberry");

//se registra el contexto de la base de datos
builder.Services.AddDbContext<ContextoBaseDatos>(opciones =>
{
    //para que use MariaDB con la cadena de conexión y ServerVersion.Autodetect identifica la versión de la Raspberry
    opciones.UseMySql(cadena, ServerVersion.AutoDetect(cadena));
});

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var jwtConfig = builder.Configuration.GetSection("Jwt");
var secretKey = jwtConfig["Key"];

// 2. Configuramos el servicio de Autenticación
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false; // Como usamos túneles de desarrollo, lo dejamos en false por ahora
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = jwtConfig["Issuer"],
        ValidateAudience = true,
        ValidAudience = jwtConfig["Audience"],
        ValidateLifetime = true,
        ClockSkew = TimeSpan.Zero // Para que el token expire exactamente a la hora indicada sin tiempo de gracia
    };
});

builder.Services.AddAuthorization(); // Encendemos el sistema de autorizaciones

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

//esta línea activa la regla CORS que se definió al principio
app.UseCors("PermitirTodo");
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
