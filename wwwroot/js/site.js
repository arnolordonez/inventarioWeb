document.addEventListener("DOMContentLoaded", function () {
    const generoSelect = document.getElementById("ID_Genero");
    const referenciaSelect = document.getElementById("ID_Referencia");
    const tallaSelect = document.getElementById("ID_Talla");

    // Seguridad: si no existe alguno, salir
    if (!generoSelect || !referenciaSelect || !tallaSelect) return;

    function filtrarOpciones(selectElement) {
        const generoSeleccionado = generoSelect.value;

        Array.from(selectElement.options).forEach(opt => {
            const idGenero = opt.dataset.idGenero;

            if (!idGenero || !generoSeleccionado || idGenero === generoSeleccionado) {
                opt.style.display = "";
            } else {
                opt.style.display = "none";
            }
        });

        // Si la opción seleccionada quedó oculta, limpiar selección
        if (
            selectElement.selectedOptions.length &&
            selectElement.selectedOptions[0].style.display === "none"
        ) {
            selectElement.value = "";
        }
    }

    generoSelect.addEventListener("change", function () {
        filtrarOpciones(referenciaSelect);
        filtrarOpciones(tallaSelect);
    });

    // Filtrar al cargar la página
    filtrarOpciones(referenciaSelect);
    filtrarOpciones(tallaSelect);
});
