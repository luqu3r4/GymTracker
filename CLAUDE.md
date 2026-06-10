# GymTracker — Contexto del proyecto

Aplicación de escritorio **WPF / C# / .NET 8** para gestión de gimnasio. Proyecto final de la asignatura de Interfaces. El repositorio está en https://github.com/luqu3r4/GymTracker.

---

## Stack técnico

- **WPF + .NET 8** (`net8.0-windows`, `UseWPF=true`)
- **MySQL 8.0** — credenciales: `root` / `alumno`, base de datos: `gymtracker`
- **MySqlConnector 2.6.0** (NuGet) — preferido sobre MySql.Data
- **QuestPDF 2024.12.0** (NuGet) — generación de informes PDF
- **Patrón MVVM** — `ViewModelBase` (INotifyPropertyChanged), `RelayCommand` (ICommand)
- **Singleton** — `DatabaseConnection.cs` gestiona el pool de conexiones

---

## Base de datos

Scripts SQL en `Database/`:
- `gymtracker_db.sql` — esquema inicial: 4 tablas, 2 vistas, 10 SPs, datos de ejemplo
- `gymtracker_rutinas.sql` — migración: tablas `rutinas` y `rutina_ejercicios`, FK en `clientes`, 8 SPs nuevos
- `gymtracker_imagenes.sql` — migración: `ejercicios.foto` cambiado a `MEDIUMBLOB`, 3 SPs nuevos

Para ejecutar una migración:
```
"C:\Program Files\MySQL\MySQL Server 8.0\bin\mysql.exe" -u root -palumno gymtracker < Database/nombre_script.sql
```

### Tablas
| Tabla | Descripción |
|---|---|
| `entrenadores` | id, nombre, pin VARCHAR(4) |
| `clientes` | id, nombre, id_entrenador FK, id_rutina_actual FK nullable |
| `ejercicios` | id, nombre, foto MEDIUMBLOB NULL |
| `seguimiento` | id, id_cliente FK CASCADE, id_ejercicio FK RESTRICT, peso DECIMAL, repeticiones INT, fecha DATE |
| `rutinas` | id, nombre, id_entrenador FK |
| `rutina_ejercicios` | id_rutina FK, id_ejercicio FK CASCADE, series INT, reps_objetivo INT |

### Vistas
- `v_seguimiento_detalle` — JOIN de las 4 tablas con nombres legibles
- `v_rutina_cliente` — rutina activa de cada cliente con detalle

### Stored Procedures relevantes
- `sp_login_entrenador(p_pin)` — devuelve id y nombre
- `sp_listar_ejercicios` — devuelve id, nombre, foto (BLOB)
- `sp_listar_ejercicios_rutina(p_id_rutina)` — devuelve id, nombre, series, reps_objetivo, foto (BLOB)
- `sp_crear_ejercicio(p_nombre, p_foto MEDIUMBLOB)`
- `sp_actualizar_ejercicio(p_id, p_nombre, p_foto MEDIUMBLOB)`
- `sp_eliminar_ejercicio(p_id)`
- CRUD para clientes, seguimiento, rutinas — ver scripts SQL

---

## Arquitectura de ficheros

```
GymTracker/
├── App.xaml / App.xaml.cs          — StartupUri=LoginWindow, QuestPDF license
├── Session.cs                       — static Session.EntrenadorActivo (Entrenador?)
├── Models/
│   ├── Entrenador.cs                — IdEntrenador, Nombre
│   ├── Cliente.cs                   — IdCliente, Nombre, IdEntrenador, IdRutinaActual (int?)
│   ├── Ejercicio.cs                 — IdEjercicio, Nombre, Foto (byte[]?)
│   ├── RutinaEjercicio.cs           — IdEjercicio, NombreEjercicio, Series, RepsObjetivo, Foto (byte[]?)
│   ├── Rutina.cs                    — IdRutina, Nombre, IdEntrenador
│   └── Seguimiento.cs               — IdSeguimiento, IdCliente, IdEjercicio, NombreEjercicio,
│                                      Peso (decimal), Repeticiones, Fecha (DateTime)
├── Data/
│   ├── DatabaseConnection.cs        — Singleton, ConnectionString usa root/alumno
│   ├── EntrenadorRepository.cs      — GetAll(), VerificarLogin(id, pin)
│   ├── ClienteRepository.cs         — GetByEntrenador, Crear, Eliminar
│   ├── EjercicioRepository.cs       — GetAll, Crear, Actualizar, Eliminar (todos con BLOB)
│   ├── SeguimientoRepository.cs     — GetByClienteEjercicio, Crear, Eliminar
│   └── RutinaRepository.cs          — GetByEntrenador, Crear, Eliminar, GetEjercicios,
│                                      AgregarEjercicio, QuitarEjercicio,
│                                      AsignarRutinaCliente, QuitarRutinaCliente
├── ViewModels/
│   ├── ViewModelBase.cs             — INotifyPropertyChanged + SetProperty<T>
│   ├── RelayCommand.cs              — ICommand con Func<bool> canExecute
│   ├── LoginViewModel.cs            — Entrenadores ComboBox, PIN, MensajeError
│   ├── ClientesViewModel.cs         — Panel principal: clientes, rutinas, ejercicios rutina
│   ├── RegistrosViewModel.cs        — Historial seguimiento + GenerarInforme PDF
│   ├── RutinasViewModel.cs          — Gestión de rutinas y sus ejercicios
│   └── EjerciciosViewModel.cs       — CRUD ejercicios con imagen, ElegirImagenAction callback
├── Views/
│   ├── LoginWindow.xaml/.cs         — ResizeMode=CanResize, MinHeight/Width=380
│   ├── ClientesWindow.xaml/.cs      — Panel principal post-login
│   ├── RegistrosWindow.xaml/.cs     — Historial de pesos de un cliente+ejercicio
│   ├── RutinasWindow.xaml/.cs       — Gestión de rutinas (ShowDialog desde ClientesWindow)
│   ├── EjerciciosWindow.xaml/.cs    — Gestión de ejercicios con imagen (ShowDialog)
│   └── NombreDialog.xaml/.cs        — Diálogo reutilizable para pedir un nombre (Resultado: string)
├── Converters/
│   └── ByteArrayToImageConverter.cs — IValueConverter byte[] → BitmapImage para WPF binding
└── Reports/
    └── ReporteService.cs            — QuestPDF: genera PDF con historial y lo abre desde el Escritorio
```

---

## Patrones clave

**Acciones/callbacks entre View y ViewModel** (mantiene MVVM limpio):
```csharp
// En el ViewModel:
public Action? AbrirRutinasAction { get; set; }
public Func<string?>? PedirNombreAction { get; set; }
public Func<byte[]?>? ElegirImagenAction { get; set; }

// En el code-behind:
vm.AbrirRutinasAction = () => { new RutinasWindow { Owner = this }.ShowDialog(); vm.CargarDatos(); };
vm.ElegirImagenAction = () => { /* OpenFileDialog + ResizarYComprimir */ };
```

**Imágenes BLOB**: Las imágenes se redimensionan a 400px y se comprimen a JPEG 80% en `EjerciciosWindow.xaml.cs` antes de guardar en DB. El converter `ByteArrayToImageConverter` se declara en `Window.Resources` como `{StaticResource ByteToImage}`.

**Fechas**: `Seguimiento.Fecha` es `DateTime` (no `DateOnly`) para compatibilidad con WPF DatePicker.

---

## Estado actual — qué está hecho

- [x] Login con selección de entrenador + PIN
- [x] Panel principal de clientes (ClientesWindow) con menú, StatusBar, reloj
- [x] CRUD clientes
- [x] Módulo de rutinas: crear/eliminar rutinas, añadir/quitar ejercicios, asignar a cliente
- [x] Vista de ejercicios de la rutina activa del cliente (DataGrid con thumbnail)
- [x] Registros de seguimiento (historial peso/reps por cliente+ejercicio)
- [x] Informe PDF con QuestPDF (historial ordenado + estadísticas)
- [x] Gestión de ejercicios con foto BLOB (EjerciciosWindow)
- [x] Imagen del ejercicio en header de RegistrosWindow
- [x] Patrón Singleton (DatabaseConnection)
- [x] Stored Procedures para todas las operaciones
- [x] 2 vistas SQL (v_seguimiento_detalle, v_rutina_cliente)

## Pendiente (requisitos de rúbrica)

- [ ] **Instalador/ejecutable empaquetado** — se hace manualmente en Visual Studio:
  `Build → Publish → Folder` o crear proyecto Setup con Inno Setup / MSIX.
  Esto **no se puede hacer desde Claude Code**, hay que hacerlo en el IDE.

---

## GitHub

Repo: https://github.com/luqu3r4/GymTracker  
Rama principal: `main`  
Commits con `Co-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>`
