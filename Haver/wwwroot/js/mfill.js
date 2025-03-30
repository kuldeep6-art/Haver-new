document.addEventListener('DOMContentLoaded', function () {
    // Autofill button handler
    document.getElementById('autofillButton').addEventListener('click', function (e) {
        e.preventDefault();

        // Generate random serial number and production order number
        function generateRandomNumber(length) {
            return Math.floor(Math.pow(10, length - 1) + Math.random() * 9 * Math.pow(10, length - 1)).toString();
        }

        // Random Serial Number starting with TEST-
        document.querySelector('#SerialNumber').value = 'TEST-' + generateRandomNumber(5);

        // Random Production Order Number with 7 digits
        document.querySelector('#ProductionOrderNumber').value = generateRandomNumber(7);

        // Generate realistic future dates based on Gantt chart
        function generateFutureDate(startDate, daysToAdd) {
            let date = new Date(startDate);
            date.setDate(date.getDate() + Math.floor(Math.random() * daysToAdd) + 1);
            return date.toISOString().split('T')[0]; // Format: YYYY-MM-DD
        }

        // Base start date
        let baseDate = new Date();

        // Set realistic date values
        let rToShipExp = generateFutureDate(baseDate, 7); // 7 days from today
        let assemblyExp = generateFutureDate(new Date(rToShipExp), 7); // 7 days after RToShipExp
        let assemblyStart = generateFutureDate(new Date(assemblyExp), 3); // 3 days after AssemblyExp
        let assemblyComplete = generateFutureDate(new Date(assemblyStart), 5); // 5 days after AssemblyStart
        let rToShipA = generateFutureDate(new Date(assemblyComplete), 2); // 2 days after AssemblyComplete

        // Date fields
        document.querySelector('#RToShipExp').value = rToShipExp;
        document.querySelector('#RToShipA').value = rToShipA;
        document.querySelector('#AssemblyExp').value = assemblyExp;
        document.querySelector('#AssemblyStart').value = assemblyStart;
        document.querySelector('#AssemblyComplete').value = assemblyComplete;

        // Nameplate dropdown (random yes or no)
        const nameplateOptions = ['Yes', 'No'];
        document.querySelector('#Nameplate').value = nameplateOptions[Math.floor(Math.random() * nameplateOptions.length)];

        // Numeric fields with random hours
        document.querySelector('#BudgetedHours').value = Math.floor(Math.random() * (100 - 30 + 1)) + 30; // Random between 30 and 100
        document.querySelector('#ActualAssemblyHours').value = Math.floor(Math.random() * (120 - 40 + 1)) + 40; // Random between 40 and 120
        document.querySelector('#ReworkHours').value = Math.floor(Math.random() * (10 - 1 + 1)) + 1; // Random between 1 and 10

        // Dropdown selections for SalesOrderID and MachineTypeID (set second option if available)
        let salesOrderOption = document.querySelector('#SalesOrderID option:nth-child(2)');
        let machineTypeOption = document.querySelector('#MachineTypeID option:nth-child(2)');

        if (salesOrderOption) {
            document.querySelector('#SalesOrderID').value = salesOrderOption.value;
        }
        if (machineTypeOption) {
            document.querySelector('#MachineTypeID').value = machineTypeOption.value;
        }

        // Checkboxes (randomly checked or unchecked)
        document.querySelector('[name="Media"]').checked = Math.random() > 0.5;
        document.querySelector('[name="SpareParts"]').checked = Math.random() > 0.5;
        document.querySelector('[name="Base"]').checked = Math.random() > 0.5;
        document.querySelector('[name="SparePMedia"]').checked = Math.random() > 0.5;
        document.querySelector('[name="AirSeal"]').checked = Math.random() > 0.5;
        document.querySelector('[name="CoatingLining"]').checked = Math.random() > 0.5;
        document.querySelector('[name="Disassembly"]').checked = Math.random() > 0.5;

        // Summernote fields (for PreOrder and Scope)
        if ($('#PreOrder').length) {
            $('#PreOrder').summernote('code', '<p>Random pre-order specifications generated</p>');
        } else {
            console.warn('Summernote PreOrder not found');
        }

        if ($('#Scope').length) {
            $('#Scope').summernote('code', '<p>Random scope details generated</p>');
        } else {
            console.warn('Summernote Scope not found');
        }
    });
});