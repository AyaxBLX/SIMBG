using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SIMBG.API.Datos;
using SIMBG.Compartido.Modelos;

namespace SIMBG.API.Controladores
{
    //Indica que es una api y que se ingresa por al ruta /api/usuarios
    [ApiController]
    [Route("api/[controller]")]
    public class UsuariosController : ControllerBase
    {
        private readonly ContextoBaseDatos _contexto;

        //aquí el constructor recibe la conexión que se configuró en Program.cs
        public UsuariosController(ContextoBaseDatos contexto)
        {
            _contexto = contexto;
        }

        //Método que responde a petición GET
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Usuario>>> ObtenerUsuarios()
        {
            //Contexto busca los usuarios de la Raspberry y los pone en una lista
            var lista = await _contexto.Usuarios.ToListAsync();

            //Se entrega la lista con un código 200, es decir, todo Ok
            return Ok(lista);
        }

        //Método que recibe datos desde la página web para crear un nuevo usuario
        [HttpPost]
        public async Task<ActionResult<Usuario>> CrearUsuario(Usuario nuevoUsuario)
        {
            //Se agrega el nuevo usuario a la base de datos
            _contexto.Usuarios.Add(nuevoUsuario);
            await _contexto.SaveChangesAsync(); //se envía instrucción a la base de datos para guardar el nuevo usuario
            //Se responde con un código 201, es decir, que se creó el recurso
            return Ok(nuevoUsuario);
        }

    }
}
