Bondi Driver

Este es el repositorio central para el desarrollo de **Bondi Driver**. Aquí centralizaremos todos los avances del proyecto, assets y scripts de Unity.

Flujo de Trabajo

Para mantener la estabilidad de la rama principal (`main`), todos los colaboradores debemos seguir estas reglas:

### 1. No trabajar sobre `main`
La rama `main` siempre debe contener una versión del proyecto que funcione. 
Nunca subas cambios directamente acá a menos que sea una corrección de último momento acordada por el equipo.

### 2. Crear una nueva rama para cada cambio
Antes de empezar a trabajar en una nueva funcionalidad, corrección de errores o asset, creá una rama propia:

```bash
# Asegurate de estar en main y actualizado
git checkout main
git pull origin main

# Creá tu rama con un nombre descriptivo
git checkout -b nombre-de-tu-rama
