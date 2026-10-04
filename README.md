# TP Ludoteca
## Para poder ejecutar el proyecto

Por razones de seguridad, Github no deja commitear las claves de autenticación de Google. Así que, parado en el proyecto TPLudoteca se deben ejecutar los siguientes comandos en consola para poder correr la solución sin errores:

dotnet user-secrets set "GoogleKeys:ClientId" "85807004007-asamq1u541729805t8cdd6ov2hqsiom3.apps.googleusercontent.com"

dotnet user-secrets set "GoogleKeys:ClientSecret" "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"

## Usuario administrador

| Correo | Clave |
|---|---|
| administrador@yopmail.com | 4DM!Nuser |

## Instructivo para el socio

### Ingresar

Registrate con tu email y una contraseña, o ingresá directamente con tu cuenta de Google. Las cuentas nuevas quedan con el rol **Socio**.

### Alquilar un juego

1. En el inicio, hacé clic en **Alquilar nuevo juego**.
2. Buscá el juego por su nombre o categoría.
3. Hacé clic en **Alquilar** y confirmá en la ventana que aparece.

Tenés **7 días** para devolverlo. Si te pasás, se cobra un recargo del **5% del valor del juego por cada día de demora**.

### Ver mis alquileres

En el inicio, hacé clic en **Ver historial de transacciones**. Cada alquiler muestra su estado: **En curso**, **Vencido** o **Devuelto**. Con el ícono del ojo ves el detalle.

### Editar o eliminar un alquiler

Solo se puede mientras el alquiler está **En curso**:

- **Editar** (ícono del lápiz): podés cambiar el juego o la fecha de retiro.
- **Eliminar** (ícono del tacho): si finalmente no querés alquilarlo. El juego vuelve a quedar disponible.

La devolución del juego la registra el administrador.

## Instructivo para el administrador

### Juegos

En el inicio, hacé clic en **Ver juegos**. Desde ahí podés agregar un juego nuevo, editarlo (ícono del lápiz) o eliminarlo (ícono del tacho). No se puede eliminar un juego que tiene copias alquiladas.

### Categorías

En el inicio, hacé clic en **Ver categorías de juego**. Funciona igual que los juegos. No se puede eliminar una categoría que tiene juegos asociados.

### Reportes

En el inicio, hacé clic en **Reportería**, o elegí **Reportes tienda** desde tu foto de perfil. Ahí ves los indicadores y gráficos de la tienda.

### Registrar una devolución

1. En **Reportería**, hacé clic en **Maestro de alquileres**.
2. Buscá el alquiler por el socio o el juego.
3. Hacé clic en **Devolución**, elegí la fecha en que se devolvió el juego y hacé clic en **Registrar devolución**.

Si hubo demora, el recargo se calcula solo.

### Asignar un rol

1. Iniciá sesión con el usuario administrador.
2. Hacé clic en tu foto de perfil (arriba a la derecha) y elegí **Asignación de roles**. También podés entrar desde el botón **Asignar roles** del inicio.
3. Buscá el usuario por su email en la barra de búsqueda.
4. Hacé clic en el botón **Asignar rol** (ícono de persona con engranaje).
5. Elegí el rol (**Administrador** o **Socio**) y hacé clic en **Guardar**.

### Desasignar un rol

Cada usuario tiene un solo rol, así que para quitarle uno tenés que asignarle el otro. Por ejemplo, para que un usuario deje de ser **Administrador**, asignale el rol **Socio** siguiendo los mismos pasos.

### Desactivar o reactivar un usuario

En **Asignación de roles**, hacé clic en el botón rojo para desactivar un usuario y confirmá en la ventana. Mientras está desactivado no puede iniciar sesión, pero se conservan sus datos y su historial. Para reactivarlo, hacé clic en el botón verde.

### A tener en cuenta

- El cambio de rol se aplica la próxima vez que el usuario inicia sesión.
- No podés quitarte el rol de **Administrador** ni desactivarte a vos mismo.
