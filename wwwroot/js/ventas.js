$(document).ready(function () {

    let carrito = [];

    // ==========================================================
    // 🔹 AUTOCOMPLETE CLIENTE
    // ==========================================================
    $("#cliente").autocomplete({
        source: function (request, response) {
            $.get('/Ventas/BuscarClientes', { term: request.term }, function (data) {
                response($.map(data, function (item) {
                    return {
                        label: item.nombreCompleto + " - " + item.documento,
                        value: item.nombreCompleto,
                        id: item.iD_Cliente
                    };
                }));
            });
        },
        select: function (event, ui) {
            $("#ID_Cliente").val(ui.item.id);

            // feedback visual
            $("#cliente").removeClass("is-invalid").addClass("is-valid");
        },
        change: function (event, ui) {
            if (!ui.item) {
                $("#ID_Cliente").val("");
                $("#cliente").removeClass("is-valid").addClass("is-invalid");
            }
        },
        minLength: 2
    });

    // 🔹 ENTER para forzar búsqueda
    $("#cliente").keypress(function (e) {
        if (e.which === 13) {
            $("#cliente").autocomplete("search");
        }
    });

    // ==========================================================
    // 🔹 CARGAR REFERENCIAS SEGÚN GÉNERO
    // ==========================================================
    $("#genero").change(function () {
        let idGenero = $(this).val();

        $.get('/Ventas/ObtenerReferenciasPorGenero', { idGenero }, function (data) {
            let html = '<option value="">Seleccione...</option>';
            data.forEach(r => {
                html += `<option value="${r.id_Referencias}">${r.descripReferencia}</option>`;
            });
            $("#referencia").html(html);
        });
    });

    // ==========================================================
    // 🔹 MATRIZ DE TALLAS
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

            let html = `
            <table class="table table-bordered">
                <thead>
                    <tr>
                        <th>Talla</th>
                        <th>Stock</th>
                        <th>Cantidad</th>
                        <th>Precio</th>
                    </tr>
                </thead><tbody>`;

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
                        <input type="number" value="${p.precioVenta}"
                            id="precio_${p.id_Producto}" class="form-control">
                    </td>
                </tr>`;
            });

            html += `</tbody></table>
                     <button class="btn btn-primary" onclick="agregar()">Agregar</button>`;

            $('#matriz').html(html);
        });
    };

    // ==========================================================
    // 🔹 AGREGAR AL CARRITO
    // ==========================================================
    window.agregar = function () {

        $('input[id^="cant_"]').each(function () {

            let id = $(this).attr('id').split('_')[1];
            let cantidad = parseInt($(this).val()) || 0;
            let precio = parseFloat($(`#precio_${id}`).val()) || 0;

            if (cantidad > 0) {

                let existente = carrito.find(p => p.ID_Producto == id);

                if (existente) {
                    existente.Cantidad += cantidad;
                } else {
                    carrito.push({
                        ID_Producto: parseInt(id),
                        Cantidad: cantidad,
                        PrecioVenta: precio
                    });
                }
            }
        });

        pintarCarrito();
    };

    // ==========================================================
    // 🔹 PINTAR CARRITO
    // ==========================================================
    function pintarCarrito() {

        let html = '';
        let total = 0;

        carrito.forEach((p, index) => {

            let subtotal = p.Cantidad * p.PrecioVenta;
            total += subtotal;

            html += `<tr>
                <td>${p.ID_Producto}</td>
                <td>${p.Cantidad}</td>
                <td>${p.PrecioVenta}</td>
                <td>${subtotal}</td>
                <td>
                    <button onclick="eliminar(${index})" class="btn btn-danger btn-sm">X</button>
                </td>
            </tr>`;
        });

        $('#carrito').html(html);
        $('#total').text(total.toLocaleString());
    }

    // ==========================================================
    // 🔹 ELIMINAR ITEM
    // ==========================================================
    window.eliminar = function (index) {
        carrito.splice(index, 1);
        pintarCarrito();
    };

    // ==========================================================
    // 🔹 GUARDAR VENTA
    // ==========================================================
    window.guardarVenta = function () {

        let cliente = $('#ID_Cliente').val();

        if (!cliente) {
            alert("Seleccione cliente");
            return;
        }

        if (carrito.length === 0) {
            alert("Agregue productos");
            return;
        }

        let total = 0;

        carrito.forEach(p => {
            total += p.Cantidad * p.PrecioVenta;
        });

        let venta = {
            ID_Cliente: parseInt(cliente),
            Detalles: carrito,
            Total: total,
            TotalVenta: total,
            AbonoInicial: 0
        };

        $.ajax({
            url: '/Ventas/Crear',
            method: 'POST',
            contentType: 'application/json',
            data: JSON.stringify(venta),
            success: function () {
                alert('Venta guardada correctamente');

                carrito = [];
                pintarCarrito();
                $('#matriz').html('');
                $('#cliente').val('').removeClass("is-valid");
                $('#ID_Cliente').val('');
            },
            error: function (err) {
                console.error(err);
                alert('Error al guardar');
            }
        });
    };

});