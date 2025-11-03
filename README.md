# Procesador de CFDI de Nómina

Aplicación de consola en .NET Core 3.1 para procesar archivos XML de CFDI de nómina y almacenarlos en una base de datos SQL Server.

## Requisitos Previos

1. **.NET Core 3.1 SDK** instalado
2. **SQL Server** (cualquier versión compatible)
3. **SQL Server Management Studio** (opcional, para administrar la base de datos)

## Configuración Inicial

### Opción A: Configuración Automática (Recomendado)

Ejecuta el script de configuración automática:

```bash
# PowerShell
.\configurar_bd.ps1
```

Este script:
- ✅ Verifica que SQL Server esté ejecutándose
- ✅ Prueba la conexión a la base de datos
- ✅ Crea la base de datos DescargaCfdiGFP si no existe
- ✅ Verifica que las tablas estén creadas correctamente

### Opción B: Configuración Manual

Ejecuta el script `db_cfdi.sql` en tu instancia de SQL Server para crear la base de datos y las tablas necesarias:

```sql
-- Abre SQL Server Management Studio
-- Conecta a tu instancia de SQL Server
-- Abre el archivo db_cfdi.sql
-- Ejecuta el script completo
```

### 2. Configurar la Cadena de Conexión

Edita el archivo `CFDIProcessor/Data/CfdiDbContext.cs` y actualiza la cadena de conexión en el método `OnConfiguring`:

```csharp
optionsBuilder.UseSqlServer("Server=TU_SERVIDOR;Database=DescargaCfdiGFP;User Id=TU_USUARIO;Password=TU_PASSWORD;TrustServerCertificate=True;");
```

**Ejemplos de cadenas de conexión:**

- **Autenticación de Windows:**
  ```
  Server=localhost;Database=DescargaCfdiGFP;Integrated Security=True;TrustServerCertificate=True;
  ```

- **Autenticación SQL Server:**
  ```
  Server=localhost;Database=DescargaCfdiGFP;User Id=sa;Password=TuPassword;TrustServerCertificate=True;
  ```

- **SQL Server Express:**
  ```
  Server=localhost\\SQLEXPRESS;Database=DescargaCfdiGFP;Integrated Security=True;TrustServerCertificate=True;
  ```

### 3. Compilar el Proyecto

```bash
cd CFDIProcessor
dotnet build
```

## Uso

### Opción 1: Ejecutar con Script (Más Fácil)

Simplemente haz doble clic en:

```
ejecutar.bat
```

### Opción 2: Ejecutar desde Terminal

```bash
cd CFDIProcessor
dotnet run
```

### Opción 3: Ver Tutorial Interactivo

Si es tu primera vez usando la aplicación:

```
ejemplo_uso.bat
```

Este script te mostrará:
- 📖 Instrucciones paso a paso
- 💡 Tips y recomendaciones
- ▶️ Opción para ejecutar la aplicación después

### Opción 4: Probar con Archivos de Ejemplo

Para hacer una prueba rápida con archivos XML de ejemplo:

```
probar_con_ejemplos.bat
```

---

La aplicación te pedirá la ruta de la carpeta que contiene los archivos XML de nómina:

```
=== Procesador de CFDI de Nómina ===

Ingrese la ruta de la carpeta con los archivos XML de nómina: C:\MisXMLs
```

### Proceso de Ejecución

1. La aplicación verifica la conexión a la base de datos
2. Solicita el tipo de CFDI a procesar:
   - **Opción 1:** Solo Nómina
   - **Opción 2:** Solo Ingreso y Egreso (Facturas)
   - **Opción 3:** Solo Pagos 2.0
   - **Opción 4:** Todos (detecta automáticamente el tipo)
3. Lee todos los archivos `.xml` de la carpeta especificada
4. Para cada archivo:
   - Identifica el tipo de comprobante
   - Extrae el UUID del TimbreFiscalDigital
   - Verifica si ya existe en la base de datos (evita duplicados)
   - Inserta los datos en las tablas correspondientes según el tipo:

#### CFDI de Nómina (N):
- `CFDI_Comprobante`, `CFDI_Emisor`, `CFDI_Receptor`
- `CFDI_Concepto`
- `Nomina_Detalle`, `Nomina_Percepciones`, `Nomina_Deducciones`, `Nomina_OtrosPagos`

#### CFDI de Ingreso/Egreso (I/E):
- `CFDI_Comprobante`, `CFDI_Emisor`, `CFDI_Receptor`
- `CFDI_Concepto`
- `CFDI_TrasladoConcepto` (IVA, IEPS, etc.)
- `CFDI_RetencionConcepto` (ISR, IVA retenido, etc.)

#### CFDI de Pagos 2.0 (P):
- `CFDI_Comprobante`, `CFDI_Emisor`, `CFDI_Receptor`
- `Pagos_Detalle` (totales)
- `Pagos_Pago` (información de cada pago)
- `Pagos_DoctoRelacionado` (facturas pagadas)

5. Muestra el resultado de cada archivo procesado

## Estructura del Proyecto

```
CFDIProcessor/
├── Data/
│   └── CfdiDbContext.cs          # Contexto de Entity Framework
├── Models/
│   ├── CfdiComprobante.cs        # Modelo del comprobante
│   ├── CfdiConcepto.cs           # Modelo de conceptos
│   ├── CfdiEmisor.cs             # Modelo del emisor
│   ├── CfdiReceptor.cs           # Modelo del receptor
│   ├── NominaDetalle.cs          # Modelo de detalle de nómina
│   ├── NominaPercepcion.cs       # Modelo de percepciones
│   ├── NominaDeduccion.cs        # Modelo de deducciones
│   └── NominaOtroPago.cs         # Modelo de otros pagos
├── Services/
│   └── NominaXmlProcessor.cs     # Servicio para procesar XMLs
└── Program.cs                     # Punto de entrada de la aplicación
```

## Características

### Tipos de CFDI Soportados

- ✅ **CFDI de Nómina (N)** - Complemento de Nómina 1.2
  - Percepciones, deducciones y otros pagos
  - Información completa del empleado
  
- ✅ **CFDI de Ingreso (I) y Egreso (E)** - Facturas
  - Conceptos con impuestos (traslados y retenciones)
  - Información de emisor y receptor
  
- ✅ **CFDI de Pagos 2.0 (P)** - Complemento de Pagos
  - Múltiples pagos por comprobante
  - Documentos relacionados con parcialidades
  - Totales de impuestos

### Funcionalidades Generales

- ✅ Procesa archivos XML de CFDI versión 4.0
- ✅ Valida el tipo de comprobante automáticamente
- ✅ Evita duplicados verificando el UUID
- ✅ Maneja errores de forma individual por archivo
- ✅ Extrae información completa de cada tipo de CFDI
- ✅ Almacena impuestos (traslados y retenciones) por concepto
- ✅ Utiliza Entity Framework Core para acceso a datos
- ✅ Soporte para procesamiento por tipo o automático

## Notas Importantes

1. **Formato de XML:** La aplicación está diseñada para procesar CFDI versión 4.0 con complemento de nómina versión 1.2
2. **Duplicados:** Si un UUID ya existe en la base de datos, el archivo se omite
3. **Errores:** Si un archivo tiene errores, se muestra el mensaje pero continúa con los demás archivos
4. **Namespaces XML:** Asegúrate de que tus XMLs usen los namespaces estándar del SAT

## Scripts de Ayuda

El proyecto incluye varios scripts para facilitar su uso:

| Script | Descripción | Cuándo usarlo |
|--------|-------------|---------------|
| `configurar_bd.ps1` | Configura automáticamente la base de datos | Primera vez que usas el proyecto |
| `ejecutar.bat` | Ejecuta la aplicación | Cada vez que quieras procesar XMLs |
| `ejemplo_uso.bat` | Tutorial interactivo paso a paso | Si es tu primera vez |
| `probar_con_ejemplos.bat` | Prueba con archivos XML de ejemplo | Para hacer pruebas rápidas |

## Solución de Problemas

### Error de conexión a la base de datos

- Ejecuta `configurar_bd.ps1` para verificar la configuración
- Verifica que SQL Server esté ejecutándose
- Confirma que la cadena de conexión sea correcta
- Verifica que el usuario tenga permisos en la base de datos

### No se procesan los archivos

- Verifica que los archivos sean XML válidos
- Confirma que sean CFDI de nómina (no facturas u otros tipos)
- Revisa que tengan el complemento de nómina

### Error al parsear fechas o números

- Verifica que el formato de los datos en el XML sea correcto
- Algunos campos opcionales pueden causar errores si no se manejan correctamente

## Uso Alternativo con Scaffold (Database-First)

Si prefieres generar los modelos automáticamente desde la base de datos existente:

```bash
# Primero crea la base de datos ejecutando db_cfdi.sql en SQL Server

# Luego ejecuta el comando scaffold
dotnet ef dbcontext scaffold "Server=localhost;Database=DescargaCfdiGFP;User Id=sa;Password=TuPassword;TrustServerCertificate=True;" Microsoft.EntityFrameworkCore.SqlServer -o Models -c CfdiDbContext --force
```

Este comando generará automáticamente todas las clases de modelo basándose en la estructura de la base de datos.

## Licencia

Este proyecto es de código abierto y está disponible para uso libre.
