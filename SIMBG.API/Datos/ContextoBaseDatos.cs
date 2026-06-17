using Microsoft.EntityFrameworkCore;//este framework nos conecta con la bd
using SIMBG.Compartido.Modelos;

namespace SIMBG.API.Datos
{
    public class ContextoBaseDatos : DbContext //Hereda de DbContext y será la conexión
    {
        //Constructor para que pase la contraseña e IP de Tailscale
        public ContextoBaseDatos(DbContextOptions<ContextoBaseDatos> opciones) : base(opciones)
        { 
        }
        
        //Aquí se van a mostrar los DbSets, que conectan cada clase con su respectiva tabla
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Proyecto> Proyectos { get; set; }
        public DbSet<BitacoraAsistencia> Bitacoras_Asistencia { get; set; }
        public DbSet<HuellaBiometrica> Huellas_Biometricas { get; set; }
        public DbSet<ActividadSemanal> Actividades_Semanales { get; set; }
        public DbSet<GestorDocumento> Gestor_Documentos { get; set; }
    }
}
