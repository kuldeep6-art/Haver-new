document.getElementById('autofillButton').addEventListener('click', function () {
    // Generate random 5-digit number and append to '104'
    function generateOrderNumber() {
        let randomNum = Math.floor(10000 + Math.random() * 90000); // 5 random digits
        return '104' + randomNum; // Ensure 8 digits
    }

    // Generate random future date within a given range
    function generateFutureDate(startDate, daysToAdd) {
        let date = new Date(startDate);
        date.setDate(date.getDate() + Math.floor(Math.random() * daysToAdd) + 1);
        return date.toISOString().split('T')[0]; // Return date in YYYY-MM-DD format
    }

    // Predefined company names, currencies, and shipping terms
    const companyNames = ['FMI', 'ABC Corp', 'XYZ Ltd', 'Global Tech'];
    const currencies = ['USD', 'CAD', 'EUR', 'GBP'];
    const shippingTerms = ['FOB Toronto', 'CIF New York', 'EXW Chicago', 'DDP LA'];

    // Randomly select company, currency, and shipping terms
    const randomCompany = companyNames[Math.floor(Math.random() * companyNames.length)];
    const randomCurrency = currencies[Math.floor(Math.random() * currencies.length)];
    const randomShipping = shippingTerms[Math.floor(Math.random() * shippingTerms.length)];

    // Base start date
    let baseDate = new Date();

    // Set random future dates based on Gantt logic
    let soDate = generateFutureDate(baseDate, 7); // Sales Order Date + 7 days
    let appDwgExp = generateFutureDate(new Date(soDate), 10); // Approval Drawing Expected
    let appDwgRel = generateFutureDate(new Date(appDwgExp), 5); // Approval Drawing Released
    let appDwgRet = generateFutureDate(new Date(appDwgRel), 5); // Approval Drawing Returned
    let preOExp = generateFutureDate(new Date(appDwgRet), 7); // Pre-order Expected
    let preORel = generateFutureDate(new Date(preOExp), 5); // Pre-order Released
    let engPExp = generateFutureDate(new Date(preORel), 10); // Engineering Package Expected

    // Autofill values
    document.getElementById('OrderNumber').value = generateOrderNumber();
    document.getElementById('companyNameInput').value = randomCompany;
    document.getElementById('SoDate').value = soDate;
    document.getElementById('Price').value = (Math.floor(Math.random() * (100000 - 20000 + 1)) + 20000).toString(); // Random price between 20,000 and 100,000
    document.getElementById('currencyDropdown').value = randomCurrency;
    document.getElementById('ShippingTerms').value = randomShipping;
    document.getElementById('AppDwgExp').value = appDwgExp;
    document.getElementById('AppDwgRel').value = appDwgRel;
    document.getElementById('AppDwgRet').value = appDwgRet;
    document.getElementById('PreOExp').value = preOExp;
    document.getElementById('PreORel').value = preORel;
    document.getElementById('EngPExp').value = engPExp;
});