# ItemDeco Plugin
- 我们现在使用遥测，您通过安装插件和任何其他既定政策来接受使用此工具


- If you speak another language please visit: **[English](https://github.com/itsFrankV22/ItemSuffixBelowName/blob/master/README.md)**
- 如果您使用其他语言，请访问：**[中文](https://github.com/itsFrankV22/ItemSuffixBelowName/blob/master/READMEChinese.md)**

## Ejemplos
![image](https://github.com/user-attachments/assets/6965e9a5-79b1-4c30-aeab-b4db51bb9309)
![image](https://github.com/user-attachments/assets/7331d281-1717-4141-bae7-0d43eea437ad)



## Descripción

El **Plugin Item Suffix** es una extensión para **TShock** que muestra una decoracion en el chat y como mensajes flotantes sobre el jugador, de lo que estan sosteniendo en la mano, se puede configurar en el archivo de config

## Características
- **Mostrar spryte del elemento sobre la cabeza del jugador:** Muestra el spryte del elemento que el jugador ha seleccionado sobre su cabeza, como si fuera el mod WeaponsOut
- **Mostrar el nombre del ítem**: Siempre que un jugador cambie el ítem que sostiene, el nombre de ese ítem aparecerá como un mensaje flotante sobre su cabeza. y en el chat, tambien muestra damage y se puede alternar en la config
- **Personalización de colores**: Los mensajes flotantes se mostrarán en un color predefinido (actualmente configurado en blanco osea 255,255,255).
- **Soporte para configuración externa**: El plugin ahora permite cargar la configuración desde un archivo JSON externo llamado `ItemDecoConfig.json` dentro de la carpeta `tshock/ItemDeco/` para personalizar los colores y la visualización.

## Instalación

1. **Descargar el plugin**: Obtén el archivo DLL del plugin.
2. **Colocar en la carpeta de plugins**: Copia el archivo DLL en la carpeta `plugins` de tu instalación de TShock.
3. **Reiniciar el servidor**: Reinicia el servidor para cargar el plugin.

> `ItemDecoration.dll`
> `LazyAPI.dll`
> `linq2db.dll`

## Uso

- Generalmente hago codigos PlugAndPlay asi que puedes usarlo sin tocar nada.
- Una vez instalado y activado, no se requieren comandos adicionales. El plugin comenzará a funcionar automáticamente, mostrando el nombre del ítem cuando cambies de objeto.
- El archivo de configuración `ItemDecoConfig.json` se encuentra en la carpeta `tshock/ItemDeco/` y permite personalizar los colores de los mensajes flotantes y otros parámetros. Si el archivo no existe, se generará con una configuración predeterminada.

## Permisos

No se requieren permisos específicos para usar este plugin. Todos los jugadores podrán ver el nombre del ítem que están sosteniendo.

## Requisitos

- **TShock**: Este plugin requiere tener una instalación de TShock para Terraria.
- **Versión de TShock**: Este plugin está diseñado para trabajar con la API de TShock v2.1.

## Contribuciones

Si deseas contribuir al desarrollo de este plugin, siéntete libre de bifurcar el repositorio y enviar tus mejoras o correcciones. Cualquier retroalimentación es muy apreciada.

## Autores

- **[FrankV22](https://github.com/itsFrankV22)**: Desarrollador principal.
- **[Soofa](https://github.com/Soof4)**: Colaborador.
- **[THENX](https://github.com/THEXN)**: Colaborador y soporte en chino.

## Licencia

Este proyecto está bajo la licencia MIT. Para más detalles, consulta el archivo LICENSE.

## Soporte

Si encuentras algún problema o tienes preguntas, no dudes en abrir un problema en el repositorio o contactarnos directamente.

---

¡Disfruta usando el **ItemDeco** y mejora tu experiencia en Terraria!

