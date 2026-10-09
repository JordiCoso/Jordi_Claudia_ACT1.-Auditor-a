using System;
using System.Runtime.InteropServices;
using AppInsegura.Datos;
using AppInsegura.Modelos;
using AppInsegura.Servicios;

namespace AppInsegura
{
    public class Program
    {
        private static readonly BaseDatosUsuarios baseDatos = new BaseDatosUsuarios();
        private static readonly AuthService auth = new AuthService(baseDatos);
        private static Usuario? usuarioActual = null;

        public static void Main(string[] args)
        {
            CargarUsuariosDeEjemplo();

            Console.WriteLine("=== Gestor de Usuarios y Partidas ===");
            // Los usuarios de prueba estan en el readme
            Console.WriteLine();

            bool salir = false;
            while (!salir)
            {
                MostrarMenu();
                string opcion = Console.ReadLine() ?? "";
                if (opcion == "5" && (usuarioActual == null || usuarioActual.Rol != "admin")) // Si no tiene permisos no puede escoger 5
                {
                    opcion = "";
                }

                try
                {
                    switch (opcion)
                    {
                        case "1":
                            Registrar();
                            break;
                        case "2":
                            IniciarSesion();
                            break;
                        case "3":
                            BuscarUsuario();
                            break;
                        case "4":
                            VerPerfil();
                            break;
                        case "5":
                            PanelAdministracion();
                            break;
                        case "6":
                            SincronizarConServidor();
                            break;
                        case "0":
                            salir = true;
                            break;
                        default:
                            Console.WriteLine("Opción no válida.");
                            break;
                    }
                }
                catch (Exception)
                {
                    Console.WriteLine("Ha ocurrido un error inesperado");
                    // Quitamos la explicacion entera del error
                }

                Console.WriteLine();
            }

            Console.WriteLine("Hasta luego.");
        }

        private static void CargarUsuariosDeEjemplo()
        {
            //TODO
            string rutaReadMe = Environment.GetEnvironmentVariable("RutaReadMe", EnvironmentVariableTarget.User);

            if (rutaReadMe == null)
            { Console.WriteLine("Variable de cuenta RutaReadMe no existe"); }
            else if (!File.Exists(rutaReadMe))
            { Console.WriteLine("Archivo ReadMe no existe"); }

            string[] archivoLeido = File.ReadAllLines(rutaReadMe);
            archivoLeido.Split("|");

            string nombreUsuarioPrueba = "UnkownUser";
            string contrasenaUsuarioPrueba = "UnknownPassword";
            string rolUsuarioPrueba = "UnknownRole";

            foreach (string linea in archivoLeido)
            {
                if (linea.Length < 1) { continue; }

                if (linea[0] != '|') { continue; }

                if (linea.Contains("Usuario") || linea.Contains("---")) { continue; }

                string[] palabrasEnLinea = linea.Split("|");

                for (int i = 0; i < palabrasEnLinea.Length; i++)
                {
                    string palabra = palabrasEnLinea[i];
                    if (!string.IsNullOrEmpty(palabra))
                    {
                        switch (i)
                        {
                            case 1:
                                nombreUsuarioPrueba = palabra;
                                break;
                            case 2:

                                contrasenaUsuarioPrueba = palabra;
                                break;
                            case 3:
                                rolUsuarioPrueba = palabra;
                                break;
                        }
                    }
                }

                Console.WriteLine(nombreUsuarioPrueba + contrasenaUsuarioPrueba + rolUsuarioPrueba);



            }

        }


        private static void MostrarMenu()
        {
            Console.WriteLine("------------------------------------");
            Console.WriteLine($"Usuario actual: {(usuarioActual != null ? usuarioActual.Nombre : "ninguno")}");
            Console.WriteLine("1. Registrar usuario");
            Console.WriteLine("2. Iniciar sesión");
            Console.WriteLine("3. Buscar usuario por nombre");
            Console.WriteLine("4. Ver mi perfil");
            if (usuarioActual != null && usuarioActual.Rol == "admin")
            {
                Console.WriteLine("5. Panel de administración");
            }
            Console.WriteLine("6. Sincronizar partida con el servidor");
            Console.WriteLine("0. Salir");
            Console.Write("Elige una opción: ");
        }

        private static void Registrar()
        {
            Console.Write("Nombre de usuario: ");
            string nombre = Console.ReadLine() ?? "";
            if (nombre.Length > 20 || string.IsNullOrWhiteSpace(nombre)) // Comprobamos que no sea null o espacio y menor de 20 characteres
            {
                Console.WriteLine("Nombre no valido");
                return;
            }

            Console.Write("Contraseña: ");
            string contrasena = Console.ReadLine() ?? "";
            if (contrasena.Length > 30 || string.IsNullOrWhiteSpace(contrasena)) // Comprobamos que no sea null o espacio y menor de 30 characteres
            {
                Console.WriteLine("Contrasena no valida");
                return;
            }

            Usuario nuevo = auth.Registrar(nombre, contrasena);
            Console.WriteLine($"Usuario '{nuevo.Nombre}' registrado con rol '{nuevo.Rol}'.");
        }

        private static void IniciarSesion()
        {
            Console.Write("Nombre de usuario: ");
            string nombre = Console.ReadLine() ?? "";
            if (nombre.Length > 20 || string.IsNullOrWhiteSpace(nombre)) // Comprobamos que no sea null o espacio y menor de 20 characteres
            {
                Console.WriteLine("Nombre no valido");
                return;
            }

            Console.Write("Contraseña: ");
            string contrasena = Console.ReadLine() ?? "";
            if (contrasena.Length > 30 || string.IsNullOrWhiteSpace(contrasena)) // Comprobamos que no sea null o espacio y menor de 30 characteres
            {
                Console.WriteLine("Contrasena no valida");
                return;
            }

            Usuario? usuario = auth.IniciarSesion(nombre, contrasena);
            if (usuario == null)
            {
                Console.WriteLine("Usuario o contraseña incorrectos.");
                return;
            }

            usuarioActual = usuario;
            Console.WriteLine($"Bienvenido, {usuario.Nombre}.");
        }

        private static void BuscarUsuario()
        {
            Console.Write("Nombre a buscar: ");
            string nombre = Console.ReadLine() ?? "";
            if (nombre.Length > 20 || string.IsNullOrWhiteSpace(nombre)) // Comprobamos que no sea null o espacio y menor de 20 characteres
            {
                Console.WriteLine("Nombre no valido");
                return;
            }

            Usuario? encontrado = baseDatos.BuscarPorNombre(nombre);
            Console.WriteLine(encontrado != null
                ? $"Encontrado: {encontrado.Nombre}" // No exponemos el rol de la persona que has buscado
                : "No se ha encontrado ningún usuario con ese nombre.");
        }

        private static void VerPerfil()
        {
            if (usuarioActual == null)
            {
                Console.WriteLine("Primero debes iniciar sesión.");
                return;
            }

            Console.WriteLine($"Nombre: {usuarioActual.Nombre}");
            Console.WriteLine($"Rol: {usuarioActual.Rol}");
            // Evitamos mostrar el token de sesión
        }

        private static void PanelAdministracion()
        {
            Console.WriteLine("=== PANEL DE ADMINISTRACIÓN ===");
            Console.WriteLine("Lista de usuarios registrados:");
            foreach (Usuario u in baseDatos.ListarTodos())
            {
                Console.WriteLine($" - {u.Nombre} ({u.Rol})");
            }
        }

        private static void SincronizarConServidor()
        {
            if (usuarioActual == null)
            {
                Console.WriteLine("Primero debes iniciar sesión.");
                return;
            }

            var red = new RedService();
            red.EnviarPuntuacion(usuarioActual.Nombre, 1000);
        }
    }
}
