ItemsDecoPlugin

- **No es compatible con la nueva versión de TRC de Narnia (Estamos trabajando para solucionarlo)** - ## Descripción **ItemsDeco** es una extensión para **TShock** que muestra una decoración en el chat y mensajes flotantes sobre el jugador, mostrando lo que tiene en la mano. Se puede configurar en el archivo de configuración.

## Características - **Mostrar objeto Spryte encima de la cabeza del jugador** Muestra el objeto del elemento que el jugador ha seleccionado encima de su cabeza, como si fuera el mod WeaponsOut. - **Mostrar nombre del elemento**: cada vez que un jugador cambia el elemento que tiene, el nombre de ese elemento aparecerá como un mensaje flotante sobre su cabeza y en el chat. También muestra daños y esto se puede alternar en la configuración. - **Personalización del color**: los mensajes flotantes se mostrarán en un color predefinido (actualmente configurado en blanco, es decir, 255,255,255). - **Soporte de configuración externa**: el complemento ahora permite cargar la configuración desde un archivo JSON externo llamado `ItemDecoConfig.json` dentro de la carpeta `tshock\ItemDeco` para personalizar los colores y la visualización.

## Instalación 

1. **Descargue el complemento**: obtenga el archivo DLL del complemento.
2. **Colocar en la carpeta de complementos**: Copie el archivo DLL en la carpeta `plugins` de su instalación de TShock.
3. **Reinicie el servidor**: reinicie el servidor para cargar el complemento


> `ItemDecoration.dll`
> `LazyAPI.dll`
> `linq2db.dll`

## Usos

- Generalmente hago códigos PlugAndPlay, para que puedas usarlos sin tocar nada. - Una vez instalado y activado, no se requieren comandos adicionales. El complemento comenzará a funcionar automáticamente y mostrará el nombre del elemento cuando cambie de objeto. - El archivo de configuración `ItemDecoConfig.json` se encuentra en la carpeta `tshock/ItemDeco/` y permite personalizar los colores de los mensajes flotantes y otros parámetros. Si el archivo no existe, se generará con una configuración predeterminada. ## Permisos No se requieren permisos específicos para utilizar este complemento. Todos los jugadores podrán ver el nombre del elemento que tienen. ## Requisitos - **TShock**: este complemento requiere una instalación de TShock para Terraria. - **Versión TShock**: este complemento está diseñado para funcionar con TShock API v2.1. ## Contribuciones Si desea contribuir al desarrollo de este complemento, no dude en bifurcar el repositorio y enviar sus mejoras o correcciones. Cualquier comentario es muy apreciado.

## Autores

- **[player177](https://github.com/Player177-YT)**: main translator
- **[Soofa](https://github.com/Soof4)**: Contributor.
- **[THENX](https://github.com/THEXN)**: Contributor and support in Chinese

## Licencia

Este proyecto está bajo la licencia MIT. Para obtener más detalles, consulte el archivo de LICENCIA. ##Soporte Si encuentra algún problema o tiene preguntas, no dude en abrir un problema en el repositorio o contactarnos directamente. --- ¡Disfruta usando **ItemDeco** y mejora tu experiencia Terraria!
