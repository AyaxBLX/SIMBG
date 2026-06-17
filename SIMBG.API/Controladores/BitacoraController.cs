using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMBG.API.Datos;
using SIMBG.Compartido.Modelos;

namespace SIMBG.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BitacoraController : ControllerBase
    {
        private readonly ContextoBaseDatos _contexto;

        public BitacoraController(ContextoBaseDatos contexto)
        {
            _contexto = contexto;
        }

        [HttpGet]
        public async Task<ActionResult<List<RegistroBitacoraDTO>>> ObtenerBitacora()
        {
            // Cruzamos (Join) la tabla Bitacora con Usuarios usando el UsuarioId
            var registros = await (from b in _contexto.Bitacoras_Asistencia
                                   join u in _contexto.Usuarios on b.UsuarioId equals u.Id
                                   orderby b.FechaEntrada descending // Los más recientes primero
                                   select new RegistroBitacoraDTO
                                   {
                                       Matricula = u.Matricula,
                                       NombreCompleto = u.Nombre + " " + u.ApellidoPaterno,
                                       FechaEntrada = b.FechaEntrada,
                                       FechaSalida = b.FechaSalida,
                                       // Lógica: Si no tiene fecha de salida, sigue adentro.
                                       Estado = b.FechaSalida == null ? "En Laboratorio" : "Completado"
                                   }).ToListAsync();

            return Ok(registros);
        }

        [HttpPost("escanear")]
        public async Task<IActionResult> EscanearYRegistrarAsistencia()
        {
            try
            {
                // comunicación con la Raspberry Pi a través del tunel privado de Tailscale
                using var clienteHTTP = new HttpClient();
                // IP de la Raspberry Pi en la red de Tailscale
                string ipRaspberry = "100.72.234.119";

                //aquí se dispara el script de Python a través de la minimal API que corre en la Raspberry Pi
                var respuestaHttp = await clienteHTTP.GetAsync($"http://{ipRaspberry}:5000/api/biometrico/leer");
                string jsonSensor = await respuestaHttp.Content.ReadAsStringAsync();

                // 1. DESEMPAQUETAMOS EL JSON
                var datosSensor = System.Text.Json.JsonSerializer.Deserialize<RespuestaSensorDTO>(
                    jsonSensor,
                    new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }
                );

                // 2. VERIFICAMOS SI EL SENSOR RECHAZÓ LA HUELLA
                // Solo revisamos si Exito es falso (porque ahora sabemos que si es true, no manda Mensaje)
                if (datosSensor == null || !datosSensor.Exito)
                {
                    string motivo = datosSensor?.Mensaje ?? "Huella no reconocida o sensor vacío";
                    return BadRequest($"El sensor rechazó la lectura: {motivo}");
                }

                // 3. SI FUE ÉXITO, EXTRAEMOS EL ID DIRECTAMENTE
                // Tomamos el Id limpio que nos mandó Python
                if (string.IsNullOrEmpty(datosSensor.Id) || !int.TryParse(datosSensor.Id, out int usuarioId))
                {
                    return BadRequest($"Error al leer el ID numérico. Paquete crudo: {jsonSensor}");
                }

                // 4. BUSCAMOS AL ALUMNO (En este caso, a ti, con el ID 1)
                var estudiante = await _contexto.Usuarios.FindAsync(usuarioId);
                if (estudiante == null)
                {
                    return NotFound($"El sensor leyó la huella #{usuarioId}, pero no existe en la BD.");
                }

                // 5. REGISTRAMOS LA ENTRADA O SALIDA
                var registroAbierto = await _contexto.Bitacoras_Asistencia
                    .Where(b => b.UsuarioId == usuarioId && b.FechaSalida == null)
                    .FirstOrDefaultAsync();

                if (registroAbierto != null)
                {
                    // SALIDA
                    registroAbierto.FechaSalida = DateTime.Now;
                    var diferencia = registroAbierto.FechaSalida.Value - registroAbierto.FechaEntrada;
                    registroAbierto.HorasSesion = (decimal)diferencia.TotalHours;

                    _contexto.Bitacoras_Asistencia.Update(registroAbierto);
                    await _contexto.SaveChangesAsync();

                    return Ok(new { Mensaje = $"Salida registrada para {estudiante.Nombre}" });
                }
                else
                {
                    // ENTRADA
                    var nuevaEntrada = new BitacoraAsistencia
                    {
                        UsuarioId = usuarioId,
                        FechaEntrada = DateTime.Now
                    };

                    _contexto.Bitacoras_Asistencia.Add(nuevaEntrada);
                    await _contexto.SaveChangesAsync();

                    return Ok(new { Mensaje = $"Entrada registrada para {estudiante.Nombre}" });
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Fallo de conexión interno: {ex.Message}");
            }
        }
    }

    // Esta clase sirve como "molde" para leer lo que manda la Raspberry
    public class RespuestaSensorDTO
    {
        public bool Exito { get; set; }
        public string? Mensaje { get; set; }
        // ¡Agregamos la variable exacta que manda Python!
        public string? Id { get; set; }
    }

}