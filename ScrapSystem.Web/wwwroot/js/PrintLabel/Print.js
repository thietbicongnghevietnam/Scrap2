

function printSelectedBarcodes() {
    const selected = Array.from(selectedBarcodes);
    if (selected.length === 0) {
        alert('Please select at least one barcode.');
        return;
    }

    let printContent = `
        <html>

        <head>
            <title>Print Barcode Labels</title>
            <script src="https://cdn.jsdelivr.net/npm/jsbarcode@3.11.5/dist/JsBarcode.all.min.js"></script>
            <style>
                @media print {
                    @page { margin: 1cm; }
                    .label-container { 
                        page-break-after: always; 
                        margin-bottom: 20px;
                    }
                    .table { 
                        width: 100%; 
                        border-collapse: collapse; 
                    }
                    .table th, .table td { 
                        border: 1px solid black; 
                        padding: 5px; 
                    }
                    .header-section {
                        margin-bottom: 15px;
                    }
                    .text-center { text-align: center; }
                    .fw-bold { font-weight: bold; }
                    .mb-3 { margin-bottom: 1rem; }
                    .mb-0 { margin-bottom: 0; }
                    .row { display: flex; flex-wrap: wrap; }
                    .col-2 { flex: 0 0 16.666667%; max-width: 16.666667%; }
                    .col-10 { flex: 0 0 83.333333%; max-width: 83.333333%; }
                    .col-md-6 { flex: 0 0 50%; max-width: 50%; }
                }
            </style>
        </head>
        <body>`;

    const selectedMasters = notExist.filter(item => selectedBarcodes.has(item.barcode));
    const currentMonthYear = new Date().toLocaleString('default', { month: '2-digit', year: 'numeric' }).toUpperCase();

    selectedMasters.forEach((master, index) => {
        const details = existConflictQty.filter(item => item.barcode == master.barcode);

        printContent += `
            <div class="container-fluid">
                <div class="label-container">
                    <!-- Header Section -->
                    <div class="header-section">
                        <div class="row align-items-center">
                            <div class="col-2">
                                <svg class="barcode" data-code="${master.barcode}"></svg>
                            </div>
                            <div class="col-10">
                                <h2 class="mb-3 fw-bold">LABEL HÀNG HỦY THÁNG ${currentMonthYear}</h2>
                                <div class="row">
                                    <div class="col-md-6"><strong>Section:</strong> ${master.section || 'N/A'}</div>
                                    <div class="col-md-6"><strong>Sanction:</strong> ${master.sanction || 'N/A'}</div>
                                </div>
                                <div class="text-center mt-2">
                                    <strong>Pallet No: ${master.pallet || 'N/A'}</strong>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Table Section -->
                    <div class="table-section">
                        <table class="table table-bordered mb-0">
                            <thead>
                                <tr>
                                    <th style="width: 8%">STT No.</th>
                                    <th style="width: 35%">Part name<br>(English name)</th>
                                    <th style="width: 30%">Part number</th>
                                    <th style="width: 10%">Qty</th>
                                    <th style="width: 17%">Pallet</th>
                                </tr>
                            </thead>
                            <tbody>`;

        details.forEach((detail, i) => {
            printContent += `
                                <tr>
                                    <td class="text-center">${i + 1}</td>
                                    <td>${detail.englishName || 'N/A'}</td>
                                    <td class="part-number">${detail.material || 'N/A'}</td>
                                    <td class="text-center">${detail.qty || 'N/A'}</td>
                                    <td class="text-center">${master.pallet || 'N/A'}</td>
                                </tr>`;
        });

        printContent += `
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>`;
    });

    printContent += `
            <script>
                document.querySelectorAll(".barcode").forEach(svg => {
                    JsBarcode(svg, svg.dataset.code, {
                        width: 2,
                        height: 80,
                        displayValue: true,
                        fontSize: 14,
                        margin: 8
                    });
                });
                window.onload = function() {
                    window.print();
                    window.onafterprint = function() {
                        window.close();
                    };
                };
                            </body>
            </html>
`;

    const printWindow = window.open('', '_blank');
    printWindow.document.write(printContent);
    printWindow.document.close();
}

