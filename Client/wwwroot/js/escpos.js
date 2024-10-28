const deviceProperties = [
    { 
        filters: [
            { vendorId: 1155, productId: 22339 },
        ], 
        name: "Xprinter",
        configuration:		1,
		interface:			0,
		endpoint:			1,

		language:			'esc-pos',
    }, 
    {
		filters: [
			{ vendorId: 0x0419 }
		],
		
		configuration:		1,
		interface:			0,
		endpoint:			1,

		language:			'esc-pos',
		codepageMapping:	'bixolon'
	}];

var device;
var encoder;
var lblPrinter;
var lblEncoder;
window.XPrinter = {
    Initialize: async function () {
       try {
            if (device == null) {
                device = await navigator.usb.requestDevice({ filters: deviceProperties.map(i => i.filters).reduce((a, b) => a.concat(b)) });
            }                            
            if (device.opened) {                
                console.log('connected', device.opened);
            }                   
            else {
                await device.open();            
                await device.selectConfiguration(1);
                await device.claimInterface(0); 
            }    
            encoder = new ThermalPrinterEncoder({
                language: 'esc-pos',
                width: 48,
            });
            return true;     
       } catch (error) {
           alert('Error connecting to printer:');
            console.error('Error connecting to printer:', error);
       } 
       return false;
    },
    Print: async function (type, data) {
        const receiptNo = data.order.receiptNo;
        var totalCost = 0;
        var subTotal = 0;
        var discount = 0;
        var balance = 0;
        var deliveryAmt = 0;
        var paid = 0;
        var items = [];
        data.order.productOrders.forEach(item => {
            items.push([`${item.product} x ${item.quantity}`, (item.cost * item.quantity).toLocaleString(undefined, { minimumFractionDigits: 2 })]);
        });
        deliveryAmt = data.order.deliveryAmt;
        discount = data.order.discount;
        totalCost = data.order.totalAmount;
        subTotal = data.order.subTotal;
        paid = data.order.payments.reduce((acc, item) => acc + item.amount, 0)
        balance = subTotal - paid;
        let formattedDiscount = discount.toLocaleString(undefined, { minimumFractionDigits: 2 });
        let formattedDeliveryAmt = deliveryAmt.toLocaleString(undefined, { minimumFractionDigits: 2 });
        let formattedTotalCost = totalCost.toLocaleString(undefined, { minimumFractionDigits: 2 });
        let formattedSubTotal = subTotal.toLocaleString(undefined, { minimumFractionDigits: 2 });  
        let formattedPaid = paid.toLocaleString(undefined, { minimumFractionDigits: 2 });
        let formattedBalance = balance.toLocaleString(undefined, { minimumFractionDigits: 2 });

        items.push(['', '='.repeat(10)]);
        items.push(['Total', (encoder) => encoder.bold().text(formattedTotalCost).bold()]);
        items.push(['Delivery Amount', (encoder) => encoder.bold().text(formattedDeliveryAmt).bold()]);
        items.push(['Discount', (encoder) => encoder.bold().text(formattedDiscount).bold()]);
        items.push(['Sub Total', (encoder) => encoder.bold().text(formattedSubTotal).bold()]);
        items.push(['Amount Paid', (encoder) => encoder.bold().text(formattedPaid).bold()]);
        items.push(['Balance', (encoder) => encoder.bold().text(formattedBalance).bold()]);

        let buffer = encoder
            .initialize()
            .width(2)
            .align('center')
            .bold(true)
            .line(`Ameesh Luxury`)
            .align('center')
            .size('small')
            .bold(false)
            .text(data.branch.branchAddress)
            .newline()
            .align('center')
            .size('small')
            .bold(false)
            .text(data.branch.phoneNo1)
            .newline()
            .newline()
            .align('left')
            .size('small')
            .text(`Date: ${data.order.orderDate}`)
            .newline()
            .align('left')
            .size('small')
            .text(`Name: ${data.customer.customerName}`)
            .newline()
            .newline()
            .align('center')
            .table(
                [
                    { width: 50, verticalAlign: 'top', align: 'left' },
                    { width: 14, verticalAlign: 'top', align: 'right' }
                ],
                [
                    ['Item', 'Cost'],
                ]
            ).table(
                [
                    { width: 50, verticalAlign: 'top', align: 'left' },
                    { width: 14, verticalAlign: 'top', align: 'right' }
                ],
                items
            )
            .align('center')
            .qrcode(receiptNo, 1, 7, 'h')
            .newline()
            .newline()
            .newline()
            .newline()
            .newline()
            .newline()
            .newline()
            .newline()
            .cut()
            .encode();
        try {
            await device.transferOut(1, buffer);
            device.close();
        } catch (error) {
            console.error('Error sending data:', error);
        }
    },
    PrintTest: async function () {
        var serialized = JSON.parse(labOrder);
        const type = 'Lab';
        const receiptNo = type === 'Lab' ? serialized.labOrder.receiptNo : data.order.receiptNo;        
        var totalCost = 0;
        var subTotal = 0;
        var discount = 0;
        var balance = 0;
        var paid = 0;
        var items = [];
        serialized.order.productOrders.forEach(item => {
            items.push([item.serviceName, item.cost.toFixed(2).toLocaleString()]);
        })
        discount = serialized.labOrder.discount.toFixed(2);
        totalCost = serialized.labOrder.total.toFixed(2);
        subTotal = serialized.labOrder.subTotal.toFixed(2)
        paid = subTotal - discount;
        balance = 0;
        items.push(['', '='.repeat(10)]);

        items.push(['Total', (encoder) => encoder.bold().text(totalCost).bold()]);
        items.push(['Discount', (encoder) => encoder.bold().text(discount).bold()]);
        items.push(['Sub Total', (encoder) => encoder.bold().text(subTotal).bold()]);
        items.push(['Amount Paid', (encoder) => encoder.bold().text(paid).bold()]);
        items.push(['Balance', (encoder) => encoder.bold().text(balance).bold()]);
       
        let buffer = encoder
            .codepage('auto')
            .initialize()
            .width(2)
            .align('center')            
            .bold(true)
            .line('Ameesh Luxury')
            .align('center')
            .size('small')
            .bold(false)
            .text(serialized.branch.branchAddress)
            .newline()
            .align('center')
            .size('small')
            .bold(false)
            .text(serialized.branch.phoneNo1)
            .newline()
            .newline()
            .align('left')
            .size('small')
            .text(`Date: ${type == 'Pharmacy' ? data.order.orderDate : serialized.labOrder.orderDate}`)
            .newline()
            .align('left')
            .size('small')
            .text(`Name: ${serialized.customer.customerName}`)
            .newline()
            .newline()
            .align('center')
            .table(
                [
                    { width: 50, verticalAlign: 'top', align: 'left' },
                    { width: 14, verticalAlign: 'top', align: 'right' }
                ],                    
                items
            )
            .align('center')
            .qrcode(receiptNo, 1, 7, 'h')
            .newline()
            .newline()
            .newline()
            .newline()
            .newline()
            .newline()
            .newline()
            .newline()
            .cut()
            .encode();
        try {
            await device.transferOut(1, buffer);
            device.close();
        } catch (error) {
            console.error('Error sending data:', error);
        }
    },
    PrintBill: async function(data) {
        let buffer = encoder
                        .codepage('auto')
                        .initialize()
                        .align('center')
                        .width(2)
                        .line('Ameesh Luxury')
                        .bold()
                        .newline()
                        .align('center')
                        .width(1)
                        .line(`Invoice #: ${data}`)
                        .newline()                        
                        .qrcode(data, 1, 7, 'h')
                        .newline()
                        .newline()                
                        .align('center')
                        .line(`Please present this slip to the cashier`)                                   
                        .newline()
                        .align('center')
                        .width(3)
                        .line(`VALID FOR 1 HOUR`)
                        .newline()
                        .newline()
                        .newline()
                        .newline()
                        .newline()
                        .newline()
                        .cut()
                        .encode();
        try {
            await device.transferOut(1, buffer);
            device.close();
        } catch (error) {
            console.error('Error sending data:', error);
        }
    },
    Test: function(base64) {
        var result;    
        let img = new Image();
        img.src = `data:image/png;base64,${base64}`;    

        try {
            img.onload = async function () {
                console.log('loading image..');
                result = encoder
                    .align('center')
                    .image(img, 640, 640, 'threshold')
                    .newline()
                    .cut()
                    .encode();

                await device.transferOut(1, result);
                device.close();
            }
        } catch (e) {

        }
    },
    InitLblPrinter: async function () {
        try {
             if (lblPrinter == null) {
                lblPrinter = await navigator.usb.requestDevice({ filters: deviceProperties.map(i => i.filters).reduce((a, b) => a.concat(b)) });
             }                            
             if (lblPrinter.opened) {                
                 console.log('label printer connected', lblPrinter.opened);
             }                   
             else {
                 await lblPrinter.open();            
                 await lblPrinter.selectConfiguration(1);
                 await lblPrinter.claimInterface(0); 
             }    
             lblEncoder = new ThermalPrinterEncoder({
                 language: 'esc-pos',
                 width: 48,
             });
             return true;     
        } catch (error) {
            alert('Error connecting to printer:');
             console.error('Error connecting to printer:', error);
        } 
        return false;
     },
     PrintLabel: async function (data){

        let buffer = lblEncoder
                        .initialize()
                        .width(2)
                        .line('HAND-MED PHARMACY')
                        .line(data.drug)
                        .line(data.prescription)
                        .encode();
        try {
            await device.transferOut(1, buffer);
            device.close();
        } catch (error) {
            console.error('Error sending data:', error);
        }
     },
    Dispose: function () {
        if (device != null) {
            device.close();            
        }
        if (lblPrinter != null){
            lblPrinter.close();
        }
    }
}

navigator.usb.addEventListener('connect', event => {
    // event.device will bring the connected device
    console.log('conneted',event);
});
  
navigator.usb.addEventListener('disconnect', event => {
    // event.device will bring the disconnected device
    console.log('disconneted', event);
}); 