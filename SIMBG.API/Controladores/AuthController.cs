using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMBG.API.Datos;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SIMBG.API.Controladores
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly ContextoBaseDatos _contexto;
        private readonly IConfiguration _configuracion;

        // Inyectamos la base de datos y la configuración (appsettings.json)
        public AuthController(ContextoBaseDatos contexto, IConfiguration configuracion)
        {
            _contexto = contexto;
            _configuracion = configuracion;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO loginRequest)
        {
            // 1. Buscamos al usuario por correo y contraseña en MariaDB
            var usuario = await _contexto.Usuarios
                .Where(u => u.Correo == loginRequest.Correo && u.PasswordHash == loginRequest.Password)
                .FirstOrDefaultAsync();

            if (usuario == null)
            {
                return Unauthorized(new { Exito = false, Mensaje = "Credenciales incorrectas" });
            }

            // 2. Si el usuario no es Administrador, le negamos el pase al panel
            if (usuario.Rol != "Administrador")
            {
                return Forbid("No tienes permisos de administrador.");
            }

            // 3. Fabricamos la credencial con sus datos (Claims)
            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, usuario.Id.ToString()),
                new Claim(ClaimTypes.Name, usuario.Nombre),
                new Claim(ClaimTypes.Role, usuario.Rol),
                new Claim(ClaimTypes.Email, usuario.Correo ?? "")
            };

            // 4. Firmamos el Token con nuestra llave secreta
            var llaveSecreta = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuracion["Jwt:Key"]));
            var credenciales = new SigningCredentials(llaveSecreta, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuracion["Jwt:Issuer"],
                audience: _configuracion["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(4), // El token durará 4 horas
                signingCredentials: credenciales
            );

            string tokenString = new JwtSecurityTokenHandler().WriteToken(token);

            // 5. Entregamos el token al Frontend
            return Ok(new
            {
                Exito = true,
                Token = tokenString,
                Usuario = usuario.Nombre,
                Rol = usuario.Rol
            });
        }
    }

    // El DTO o "Molde" para recibir los datos desde el navegador web
    public class LoginDTO
    {
        public string Correo { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}