using System.Diagnostics;

var builder = WebApplication.CreateBuilder(args);

// debe habilitarse CORS para que el frontend pueda vincularse con esta API desde un puerto diferente dentro de la red local
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        policy => policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();
app.UseCors("AllowAll");

// Liga para enlace externo para los sinodales
app.MapGet("/api/status", () =>
    Results.Ok(new
    {
        sistema = "SIMBG API v1.0",
        estado = "En línea y listo",
        plataforma = "Raspberry Pi 5 (.NET 10)",
        horaServidor = DateTime.Now.ToString("HH:mm:ss")
    }));

// Endpoint para Leer Huella
app.MapGet("/api/biometrico/leer", () =>
{
    try
    {
        // Ruta absoluta hacia script en la Raspberry Pi
        var resultado = EjecutarScriptPython("/home/raspbialix/leer_huella.py");
        resultado = resultado.Trim();

        // Asumiendo que el script de Python imprime "ID:5" en caso de éxito
        if (resultado.StartsWith("ID:"))
        {
            string idExtraido = resultado.Replace("ID:", "");
            return Results.Ok(new { exito = true, id = idExtraido });
        }
        else
        {
            return Results.BadRequest(new { exito = false, mensaje = resultado });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error interno del servidor satélite: {ex.Message}");
    }
});

//Endpoint para Enrolar Huella
app.MapPost("/api/biometrico/enrolar/{id}", (int id) =>
{
    try
    {
        // El ID capturado en la URL como argumento va al script de Python
        var resultado = EjecutarScriptPython($"/home/raspbialix/enrolar.py {id}");
        resultado = resultado.Trim();

        // El script imprime "EXITO" al terminar
        if (resultado == "EXITO")
        {
            return Results.Ok(new { exito = true, mensaje = $"Huella guardada en posición {id}" });
        }
        else
        {
            return Results.BadRequest(new { exito = false, mensaje = resultado });
        }
    }
    catch (Exception ex)
    {
        return Results.Problem($"Error interno: {ex.Message}");
    }
});

// Esto configura la escucha en todas las interfaces de red locales
app.Run("http://0.0.0.0:5000");


// Método auxiliar para el puente entre C# y el hardware
static string EjecutarScriptPython(string rutaYArgumentos)
{
    var process = new Process();

    // Se dirige al ejecutable de Python del entorno virtual para
    // asegurar que tenga acceso a la librería adafruit-fingerprint
    process.StartInfo.FileName = "/usr/bin/python3";

    process.StartInfo.Arguments = rutaYArgumentos;
    process.StartInfo.UseShellExecute = false;
    process.StartInfo.RedirectStandardOutput = true;
    process.StartInfo.RedirectStandardError = true;

    process.Start();

    // Capturamos lo que el script imprima en consola
    string output = process.StandardOutput.ReadToEnd();
    string error = process.StandardError.ReadToEnd();
    process.WaitForExit();

    // Si hubo un error a nivel de Python se lanza la excepción
    if (!string.IsNullOrEmpty(error) && string.IsNullOrEmpty(output))
    {
        throw new Exception(error);
    }

    return output;
}