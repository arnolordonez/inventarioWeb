// ==========================================================
// 🔥 ventas.js - VERSION ESTABLE FINAL
// ==========================================================

console.log("🔥 JS CARGADO - FUERA DE JQUERY");

// ==========================================================
// 🧪 TEST GLOBAL
// ==========================================================
window.testJS = function () {
    document.getElementById("testJSResult").innerText = "✅ JS funcionando correctamente";
};


// ==========================================================
// 🛒 CARRITO GLOBAL REAL (FUERA DEL DOM READY)
// ==========================================================
let carrito = [];
window.carrito = carrito;

// ==========================================================
// 🔹 CUANDO CARGA EL DOM
// ==========================================================
$(function () {

    console.log("ventas.js activo");

    // ✅ UN SOLO CARRITO GLOBAL (NO DUPLICAR)
    // let carrito = [];

    // 👉 HACERLO GLOBAL PARA TODAS LAS FUNCIONES
   // window.carrito = carrito;

    // ==========================================================
    // 🔍 BUSCAR CLIENTE
    // ==========================================================
    window.buscarCliente = function () {

        let term = $("#txtBuscarCliente").val().trim();

        if (!term) {
            $("#clienteError").text("Ingrese nombre o cedula").show();
            return;
        }

        console.log("Buscando cliente:", term);

        $.ajax({
            url: '/Ventas/BuscarClientes',
            type: 'GET',
            dataType: 'json',
            data: { term: term },

            success: function (data) {

                console.log("Respuesta servidor:", data);

                if (!data || data.length === 0) {
                    $("#clienteError").text("Cliente no encontrado").show();
                    $("#listaClientes").html("");
                    return;
                }

                let html = `<div class="list-group">`;

                data.forEach(function (c) {
                    let nombre = (c.nombreCompleto || "").replaceAll("'", "");
                   
                    //let nombre = (c.nombreCompleto || "").replace(/'/g, ""); 
                    let cedula = c.cedula || "";

                    // 🔥 NORMALIZAR DATOS
                    let tieneDeuda = (c.tieneDeuda === true || c.tieneDeuda === 1 || c.tieneDeuda === "true");
                    let totalDeuda = parseFloat(c.totalDeuda) || 0;

                    html += `
                        <button type="button"
                            class="list-group-item list-group-item-action btnSeleccionarCliente"
                            data-id="${c.id_Cliente}"
                            data-nombre="${nombre}"
                            data-cedula="${cedula}"
                            data-tienedeuda="${tieneDeuda ? 1 : 0}"
                            data-totaldeuda="${totalDeuda}">

                            <strong>${nombre}</strong><br>
                            <small>Cedula: ${cedula}</small><br>
                            <small style="color:${tieneDeuda ? 'red' : 'green'}">
                                ${tieneDeuda ? `DEUDA $${totalDeuda.toLocaleString()}` : 'PAZ Y SALVO'}
                            </small>
                        </button>`;
                });

                html += `</div>`;

                $("#listaClientes").html(html);
                $("#clienteError").hide();
            },

            error: function (err) {
                console.error("ERROR AJAX:", err);
                alert("Error al buscar cliente");
            }
        });
    };




    // ==========================================================
    // 🔹 CARGAR COMBOS (FILTROS)
    // ==========================================================

    // 🔹 GENEROS
    $.get('/Ventas/ObtenerGeneros', function (data) {

        console.log("Generos RAW:", data);

        let combo = $('#genero');
        combo.empty();
        combo.append('<option value="">Seleccione género</option>');

        if (!data || data.length === 0) {
            console.warn("⚠️ Generos vacío");
            return;
        }

        data.forEach(item => {

            let value = item.Value ?? item.value ?? item.idGenero ?? item.ID_Genero;
            let text = item.Text ?? item.text ?? item.nombre ?? item.DescripGenero;

            console.log("Genero procesado:", value, text);

            combo.append(`<option value="${value}">${text}</option>`);
        });
    });


    // 🔹 TELAS
    $.get('/Ventas/ObtenerTelas', function (data) {

        console.log("Telas RAW:", data);

        let combo = $('#tela');
        combo.empty();
        combo.append('<option value="">Seleccione tela</option>');

        data.forEach(item => {

            let value = item.Value ?? item.value ?? item.idTela ?? item.ID_Telas;
            let text = item.Text ?? item.text ?? item.nombre ?? item.DescripTela;

            combo.append(`<option value="${value}">${text}</option>`);
        });
    });


    // 🔹 COLORES
    $.get('/Ventas/ObtenerColores', function (data) {

        console.log("Colores RAW:", data);

        let combo = $('#color');
        combo.empty();
        combo.append('<option value="">Seleccione color</option>');

        data.forEach(item => {

            let value = item.Value ?? item.value ?? item.idColor ?? item.ID_Color;
            let text = item.Text ?? item.text ?? item.nombre ?? item.Nombre;

            combo.append(`<option value="${value}">${text}</option>`);
        });
    });


    // 🔹 REFERENCIAS

    $('#genero').on('change', function () {
        let idGenero = $(this).val();

        let combo = $('#referencia');
        combo.empty();
        combo.append('<option value="">Seleccione referencia</option>');

        if (!idGenero) return;

        $.get('/Ventas/ObtenerReferenciasPorGenero', { idGenero: idGenero }, function (data) {

            console.log("Referencias RAW:", data);

            data.forEach(item => {

                let value = item.Value ?? item.value ?? item.idReferencia ?? item.ID_Referencias;
                let text = item.Text ?? item.text ?? item.nombre ?? item.DescripReferencia;

                combo.append(`<option value="${value}">${text}</option>`);
            });
        });

    });


    // 🔹 METODOS DE PAGO
    $.get('/Ventas/ObtenerMetodosPago', function (data) {

        console.log("MetodosPago RAW:", data);

        let combo = $('#metodoPago');
        combo.empty();
        combo.append('<option value="">Seleccione método de pago</option>');

        if (!data || data.length === 0) {
            console.warn("⚠️ Métodos de pago vacío");
            return;
        }

        data.forEach(item => {
            combo.append(`<option value="${item.value}">${item.text}</option>`);
        });
    });


    // ==========================================================
    // 👆 SELECCIONAR CLIENTE
    // ==========================================================
    $(document).on("click", ".btnSeleccionarCliente", function () {

        let btn = $(this);

        let id = btn.data("id");
        let nombre = btn.data("nombre");
        let cedula = btn.data("cedula");

        let tieneDeuda = Number(btn.data("tienedeuda")) === 1;
        let totalDeuda = Number(btn.data("totaldeuda")) || 0;

        console.log("Cliente seleccionado:", id);

        // Guardar ID real
        $("#ID_Cliente").val(id);

        // Mostrar datos
        $("#cliNombre").text(nombre);
        $("#cliCedula").text(cedula);

        // Estado financiero
        $("#cliEstado").removeClass("text-success text-danger");

        if (tieneDeuda) {
            $("#cliEstado")
                .text("🔴 DEUDA $" + totalDeuda.toLocaleString())
                .addClass("text-danger");

            $("#btnGuardarVenta").prop("disabled", true);
        } else {
            $("#cliEstado")
                .text("🟢 PAZ Y SALVO")
                .addClass("text-success");

            $("#btnGuardarVenta").prop("disabled", false);
        }

        // Mostrar panel cliente
        $("#clienteInfo").show();
        $("#btnCambiarCliente").show();
        $("#listaClientes").html("");

        // Bloquear búsqueda
        $("#txtBuscarCliente")
            .val(nombre)
            .prop("disabled", true)
            .addClass("is-valid");

        $("#clienteError").hide();

        // 🔥 LIMPIAR MATRIZ Y CARRITO AL CAMBIAR CLIENTE (RECOMENDADO)
        carrito.length = 0;
        pintarCarrito();
        $("#matriz").html("");
    });

    // ==========================================================
    // ⌨️ ENTER = BUSCAR
    // ==========================================================
    $("#txtBuscarCliente").on("keydown", function (e) {
        if (e.key === "Enter") {
            e.preventDefault();
            buscarCliente();
        }
    });

    // ==========================================================
    // 🔄 CAMBIAR CLIENTE (VERSIÓN COMPLETA Y FUNCIONAL)
    // ==========================================================
    window.cambiarCliente = function () {

        console.log("🔄 Reiniciando cliente y ciclo de venta...");

        // 🔹 1. Limpiar datos del cliente
        $("#txtBuscarCliente").val("").prop("disabled", false).removeClass("is-valid");
        $("#ID_Cliente").val("");

        $("#clienteInfo").hide();
        $("#btnCambiarCliente").hide();
        $("#listaClientes").html("");
        $("#clienteError").hide();

        $("#cliNombre, #cliCedula, #cliEstado").text("");

        // 🔹 2. Limpiar carrito
        if (typeof carrito !== "undefined") {
            // carrito = [];
            carrito.length = 0;
            pintarCarrito();
        }

        // 🔹 3. Limpiar matriz
        $('#matriz').html('');

        // 🔹 4. Habilitar botones de venta
        $("#btnGuardarVenta").prop("disabled", false);
        $("#btnCargarCarrito").prop("disabled", false);

        // 🔹 5. Reiniciar selects (género, referencia, talla)
        $('#genero').val('');
        $('#referencia').val('');
        $('#tela').val('');
        $('#color').val('');

        // 🔹 6. Volver a cargar data inicial si aplica
        if (typeof cargarGeneros === "function") cargarGeneros();
        if (typeof cargarReferencias === "function") cargarReferencias();

        if (typeof cargarTelas === "function") cargarTelas();
        if (typeof cargarColores === "function") cargarColores();
                
        console.log("✅ Ciclo de venta reiniciado correctamente");
    };

    // ==========================================================
    // 📊 MATRIZ
    // ==========================================================
    window.cargarMatriz = function () {

        let genero = $('#genero').val();
        let referencia = $('#referencia').val();
        let tela = $('#tela').val();
        let color = $('#color').val();

        if (!genero || !referencia || !tela || !color) {
            alert("Debe seleccionar todos los filtros");
            return;
        }

        $.get('/Ventas/ObtenerMatriz', {
            idGenero: genero,
            idReferencia: referencia,
            idTela: tela,
            idColor: color
        }, function (data) {

            if (!data || data.length === 0) {
                $('#matriz').html('<div class="alert alert-warning">No hay productos disponibles</div>');
                return;
            }

            let html = `
        <table class="table table-bordered">
            <thead>
                <tr>
                    <th>Talla</th>
                    <th>Stock</th>
                    <th>Docenas</th>
                   
                    <th>Precio Docena</th>
                </tr>
            </thead>
            <tbody>`;

            data.forEach(p => {
                html += `
            <tr>
                <td>${p.talla}</td>
                <td>${p.stock}</td>
                <td>
                    <input type="number" min="0" value="0"
                        id="cant_${p.id_Producto}" class="form-control">
                </td>
               <td>
                    <!-- ESTA COMENTADA <input type="number" value="${p.precioVTA}"> -->

                    <input type="number" value="${p.precioVTA}" min="0" step="1"
                     id="precio_${p.id_Producto}" class="form-control">
                </td>
            </tr>`;
            });

            html += `
            </tbody>
        </table>

        <button class="btn btn-success mt-2" onclick="agregar()">Agregar al carrito</button>
        <button class="btn btn-secondary mt-2 ms-2" onclick="limpiarMatriz()">Nuevo producto</button>
        `;

            $('#matriz').html(html);
        });
    };


    // ==========================================================
    // ➕ AGREGAR AL CARRITO (LIMPIO)
    // ==========================================================
    window.agregar = function () {

        let seAgrego = false;

        // ==========================================================
        // 🔥 RECORRER INPUTS DE CANTIDAD (DOCENAS)
        // ==========================================================
        $('input[id^="cant_"]').each(function () {

            // 🔹 Obtener ID del producto
            let id = $(this).attr('id').split('_')[1];

            // 🔹 Cantidad ahora es DOCENAS (ya no unidades)
            let docenas = parseInt($(this).val()) || 0;

            // 🔹 Precio ya es por DOCENA (ej: 35000)

            let precioDocena = parseInt($(`#precio_${id}`).val()) || 0;
            // ✅ 🔴 AQUÍ VA LA VALIDACIÓN
            if (precioDocena <= 0) {
                alert("Precio inválido");
                seAgrego = false;
                return false; // 🔥 corta el each correctamente
            }

            /*
            if (precioDocena <= 0) {
                alert("Precio inválido");
                return;
            }
            */

            if (docenas > 0) {

                seAgrego = true;

                // 🔥 NUEVO MODELO (SIN CONVERSIONES)
                // ✔ Cantidad = docenas
                // ✔ Precio = precio por docena
                // ✔ Subtotal = docenas * precio docena
                let cantidad = docenas;
                let subtotal = Math.round(cantidad * precioDocena);

                // 🔍 BUSCAR SI YA EXISTE EN CARRITO
                let existente = window.carrito.find(p => p.ID_Producto == id);

                if (existente) {

                    // ✅ ACUMULAR DOCENAS (NO MULTIPLICAR POR 12)
                    existente.Cantidad += cantidad;

                    // 🔥 RECALCULAR DIRECTO (SIN AJUSTES)
                    existente.PrecioVenta = precioDocena;
                    existente.Subtotal = existente.Cantidad * existente.PrecioVenta;

                } else {

                    // ✅ AGREGAR NUEVO PRODUCTO (MODELO LIMPIO)
                    window.carrito.push({
                        ID_Producto: parseInt(id),
                        Cantidad: cantidad,              // ✔ docenas
                        PrecioVenta: precioDocena,       // ✔ precio docena
                        Subtotal: subtotal               // ✔ total directo
                    });
                }
            }
        });

        // 🔴 VALIDACIÓN
        if (!seAgrego) {
            alert("Debe ingresar al menos una cantidad");
            return;
        }

        // 🔄 ACTUALIZAR UI
        pintarCarrito();

        // 🧹 LIMPIAR SOLO INPUTS (NO BORRAR CARRITO)
        $('input[id^="cant_"]').val('');

        console.log("🛒 CARRITO ACTUAL:", window.carrito);
    };


    // ==========================================================
    // 🧹 LIMPIAR MATRIZ (CLAVE PARA MULTIPLES PEDIDOS)
    // ==========================================================
    window.limpiarMatriz = function () {

        // limpiar inputs
        $('input[id^="cant_"]').val('');


        // limpiar filtros (flujo rápido)
        $('#referencia').val('');
        $('#tela').val('');
        $('#color').val('');

        // limpiar vista
        $('#matriz').html('');

        console.log("🧹 Matriz lista para nuevo producto");
    };


    // ==========================================================
    // 🛒 PINTAR CARRITO (MODELO POR DOCENAS - SIN DECIMALES)
    // ==========================================================
    function pintarCarrito() {

        let html = '';
        let total = 0;

        carrito.forEach((p, index) => {

            // 🔥 USAR SUBTOTAL YA CALCULADO (NO REPROCESAR)
            let subtotal = p.Subtotal;

            // 🔥 ACUMULAR TOTAL (ENTERO)
            total += subtotal;

            // 🔥 MODELO NUEVO
            let docenas = p.Cantidad;        // ✔ ya son docenas
            let precioDocena = p.PrecioVenta; // ✔ precio por docena

            html += `
        <tr>
            <td>${p.ID_Producto}</td>
            <td>${docenas}</td>
            <td>${precioDocena.toLocaleString()}</td>
            <td>${subtotal.toLocaleString()}</td>
            <td>
                <button onclick="eliminar(${index})" class="btn btn-danger btn-sm">X</button>
            </td>
        </tr>`;
        });

        $('#carrito tbody').html(html);

        // ==========================================================
        // 🔥 IVA Y TOTALES (SIN DECIMALES)
        // ==========================================================

        // 🔥 IVA REDONDEADO A ENTERO
        let iva = Math.round(total * 0.19);

        // 🔥 TOTAL FINAL ENTERO
        let totalFinal = total + iva;

        // 🔥 MOSTRAR SIN DECIMALES
        $('#totalSinIva').val(total);
        $('#totalVenta').val(totalFinal);

        // ==========================================================
        // 💰 TIPO DE VENTA
        // ==========================================================
        if ($('#tipoVenta').val() === "CONTADO") {
            $('#abonoInicial')
                .val(totalFinal)
                .prop('disabled', true);
        }
    }

    // ==========================================================
    // 📦 PINTAR PEDIDOS (MODELO DOCENAS - SIN DECIMALES)
    // ==========================================================
    function pintarPedidos() {

        let html = '';

        pedidos.forEach((pedido, index) => {

            // 🔥 USAR SUBTOTAL YA CALCULADO (EVITA ERRORES)
            let totalPedido = pedido.reduce((sum, p) =>
                sum + (p.Subtotal || (p.Cantidad * p.PrecioVenta)), 0);

            html += `
        <div class="card mb-2 p-2">

            <strong>Pedido #${index + 1}</strong>

            <div>
                💰 Total: $${totalPedido.toLocaleString()}
            </div>

            <div>
                📦 Productos: ${pedido.length}
            </div>

        </div>`;
        });

        $('#listaPedidos').html(html);
    }


    // ==========================================================
    // ❌ ELIMINAR
    // ==========================================================
    window.eliminar = function (index) {
        carrito.splice(index, 1);
        pintarCarrito();
    };


    // ==========================================================
    // 🔄 TIPO DE VENTA (SIN DECIMALES - MODELO DOCENAS)
    // ==========================================================
    $('#tipoVenta').off('change').on('change', function () {

        let tipo = $(this).val();

        console.log("🔥 EVENTO CHANGE DISPARADO");
        console.log("TipoVenta:", tipo);

        if (tipo === "CONTADO") {

            // 🔴 VALIDACIÓN: carrito no puede estar vacío
            if (!carrito || carrito.length === 0) {
                alert("Debe agregar productos antes de seleccionar CONTADO");
                $('#tipoVenta').val('');
                return;
            }

            // ==========================================================
            // 🔥 TOTAL YA CONSOLIDADO (NO REPROCESAR)
            // ==========================================================
            let total = carrito.reduce((sum, p) => sum + p.Subtotal, 0);

            // 🔥 IVA ENTERO (SIN DECIMALES)
            let iva = Math.round(total * 0.19);

            // 🔥 TOTAL FINAL ENTERO
            let totalFinal = total + iva;

            console.log("TotalFinal REAL:", totalFinal);

            // ==========================================================
            // 💰 ABONO = TOTAL COMPLETO
            // ==========================================================
            $('#abonoInicial')
                .val(totalFinal)
                .prop('disabled', true);

        } else {

            // ==========================================================
            // 🔄 LIMPIAR ABONO (CRÉDITO)
            // ==========================================================
            $('#abonoInicial')
                .val('')
                .prop('disabled', false);
        }
    });



    // ==========================================================
    // 💾 GUARDAR VENTA (MODELO ENTERO - LIMPIO)
    // ==========================================================
    window.guardarVenta = function () {

        console.log("🔥 CLICK EN GUARDAR");

        let cliente = $('#ID_Cliente').val();
        let metodoPago = $('#metodoPago').val();
        let tipoVenta = $('#tipoVenta').val();
        let abonoInput = $('#abonoInicial').val();

        // ==========================================================
        // 🔴 VALIDACIONES
        // ==========================================================
        if (!cliente) {
            $("#clienteError").text("Debe seleccionar un cliente").show();
            return;
        } else {
            $("#clienteError").hide();
        }

        if (!carrito || carrito.length === 0) {
            alert("Agregue productos");
            return;
        }

        if (tipoVenta !== "CONTADO" && !metodoPago) {
            alert("Seleccione método de pago");
            return;
        }

        // ==========================================================
        // 🔥 TOTALES (MODELO ENTERO)
        // ==========================================================
        let total = carrito.reduce((sum, p) => sum + p.Subtotal, 0);

        let iva = Math.round(total * 0.19);
        let totalFinal = total + iva;

        let abono = (tipoVenta === "CONTADO")
            ? totalFinal
            : (parseInt(abonoInput) || 0);

        // ==========================================================
        // 📦 OBJETO VENTA
        // ==========================================================
        let venta = {
            ID_Cliente: parseInt(cliente),
            ID_MetodoPago: metodoPago ? parseInt(metodoPago) : null,
            AbonoInicial: abono,
            TotalVenta: totalFinal,
            TipoVenta: tipoVenta,

            Detalles: carrito.map(p => ({
                ID_Producto: p.ID_Producto,
                Cantidad: p.Cantidad,
                PrecioVenta: p.PrecioVenta,
                Subtotal: p.Subtotal
            }))
        };

        console.log("🔥 VENTA A ENVIAR:", venta);

        // ==========================================================
        // 🚀 AJAX (ÚNICO BLOQUE - SIN DUPLICADOS)
        // ==========================================================
        $.ajax({
            url: '/Ventas/Crear',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(venta),

            beforeSend: function () {
                console.log("🚀 Enviando venta al backend...");
            },

            success: function (res) {
                console.log("✅ RESPUESTA OK:", res);

                if (res.success) {

                    // 🖨️ IMPRIMIR ORDEN DE PRODUCCIÓN
                    imprimirOrdenDesdeBD(res.idPedido);

                } else {
                    alert("Error al guardar la venta");
                }
            },

            error: function (err) {
                console.error("❌ ERROR:", err);
                console.error("❌ RESPUESTA:", err.responseText);

                alert("Error: " + err.responseText);
            }
        });
    }


    // ==========================================
    // 🖨️ ORDEN DE PRODUCCIÓN DESDE BD
    // ==========================================
    function imprimirOrdenDesdeBD(idPedido) {

        // 📄 Genera y abre el PDF
        window.open(`/Ventas/GenerarOrdenProduccionPDF?idPedido=${idPedido}`, '_blank');

        // 🔄 Reinicia el módulo después
        setTimeout(() => {
            location.reload();
        }, 2000);
    }

}); // ← CIERRE FINAL