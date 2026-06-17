using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using SIMBG.API.Datos;
using SIMBG.Compartido.Modelos;

namespace SIMBG.API.Controladores
{
    [Route("api/[controller]")]
    [ApiController]
    // ¡EL CANDADO! Solo los usuarios con un Token válido y Rol "Administrador" pueden entrar aquí
    [Authorize(Roles = "Administrador")]
    public class EstudiantesController : ControllerBase
    {
        private readonly ContextoBaseDatos _contexto;
        
        public EstudiantesController(ContextoBaseDatos contexto)
        {
            _contexto = contexto;
        }

        //1. Obtener la lista de estudiantes
        [HttpGet("lista")]
        public async Task<IActionResult> ObtenerEstudiantes() {
            //Filtrar solo los usuarios con rol "Estudiante"
            var estudiantes = await _contexto.Usuarios
                .Where(u => u.Rol == "Estudiante" && u.Activo == true)
                .Select(u => new
                {
                    u.Id,
                    u.Matricula,
                    u.Nombre,
                    u.ApellidoPaterno,
                    u.ApellidoMaterno,
                })
                .ToListAsync();
            return Ok(estudiantes);
        }

        //2. Crear un nuevo estudiante
        [HttpPost("nuevo")]
        public async Task<IActionResult> CrearEstudiante([FromBody] NuevoEstudianteDTO estudianteDTO)
        {
            //Verificamos si la maetrícula ya esxiste
            bool existe = await _contexto.Usuarios.AnyAsync(u => u.Matricula == estudianteDTO.Matricula);
            if (existe)
                {
                return BadRequest("Ya existe un estudiante con esa matrícula.");
            }

            var nuevoUsuario = new Usuario
            {
                Matricula = estudianteDTO.Matricula,
                Nombre = estudianteDTO.Nombre,
                ApellidoPaterno = estudianteDTO.ApellidoPaterno,
                ApellidoMaterno = estudianteDTO.ApellidoMaterno,
                Rol = "Estudiante", // Se fuerza el rol para que el admin no cree otros admins por error
                Activo = true, // Al ser estudiante de huella, no requiere correo ni password para el sistema
                Correo = null,
                PasswordHash = null
            };

            _contexto.Usuarios.Add(nuevoUsuario);
            await _contexto.SaveChangesAsync();
            return Ok(new { Mensaje = "Estudiante creado exitosamente.", Id = nuevoUsuario.Id });
        }
    }

    // El "Molde" para recibir los datos del frontend al crear un alumno
    public class NuevoEstudianteDTO
    {
        public string Matricula { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string ApellidoPaterno { get; set; } = string.Empty;
        public string ApellidoMaterno { get; set; } = string.Empty;
    }
}
