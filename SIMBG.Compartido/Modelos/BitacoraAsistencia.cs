using System.ComponentModel.DataAnnotations;

namespace SIMBG.Compartido.Modelos
{
    public class BitacoraAsistencia
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UsuarioId { get; set; }

        [Required]
        public DateTime FechaEntrada { get; set; } = DateTime.Now;

        //puede estar vacío hasta que el alumno registre su salida, por eso es nullable
        public DateTime? FechaSalida { get; set; }

        //se calcula al registrar la salida, por eso es nullable
        public decimal? HorasSesion { get; set; }


    }
}
