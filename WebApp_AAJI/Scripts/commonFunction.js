
function threedigit(word) {
    eja = Array("Nol", "Satu", "Dua", "Tiga", "Empat", "Lima", "Enam", "Tujuh", "Delapan", "Sembilan");
    while (word.length < 3) word = "0" + word;
    word = word.split("");
    a = word[0]; b = word[1]; c = word[2];
    word = "";
    word += (a != "0" ? (a != "1" ? eja[parseInt(a)] : "Se") : "") + (a != "0" ? (a != "1" ? " Ratus" : "ratus") : "");
    word += " " + (b != "0" ? (b != "1" ? eja[parseInt(b)] : "Se") : "") + (b != "0" ? (b != "1" ? " Puluh" : "puluh") : "");
    word += " " + (c != "0" ? eja[parseInt(c)] : "");
    word = word.replace(/Sepuluh ([^ ]+)/gi, "$1 Belas");
    word = word.replace(/Satu Belas/gi, "Sebelas");
    word = word.replace(/^[ ]+$/gi, "");

    return word;
}

function terbilang(word) {
    word = word + "";
    thousand = Array("", "Ribu", "Juta", "Milyar", "Trilyun");
    cent = word.split(".");
    word = cent[0];
    cent = cent[1] ? cent[1] : "0";

    subword = ""; i = 0;
    while (word.length > 3) {
        subdigit = threedigit(word.substr(word.length - 3, 3));
        subword = subdigit + (subdigit != "" ? " " + thousand[i] + " " : "") + subword;
        word = word.substring(0, word.length - 3);
        i++;
    }
    subword = threedigit(word) + " " + thousand[i] + " " + subword;
    subword = subword.replace(/^ +$/gi, "");

    word = (subword == "" ? "NOL" : subword.ucwords()) + " Rupiah";
    subword = threedigit(cent);
    cent = (subword == "" ? "" : " ") + subword.ucwords()/*toUpperCase()*/ + (subword == "" ? "" : " Perak");
    return word + cent;
}

String.prototype.ucwords = function () {
    str = this.toLowerCase();
    return str.replace(/(^([a-zA-Z\p{M}]))|([ -][a-zA-Z\p{M}])/g,
        function ($1) {
            return $1.toUpperCase();
        });
}

function getDate()
{
    var today = new Date();
    var dd = today.getDate();
    var mm = today.getMonth() + 1; //January is 0!
    var yyyy = today.getFullYear();

    if (dd < 10) {
        dd = '0' + dd
    }

    if (mm < 10) {
        mm = '0' + mm
    }

    today = mm + '/' + dd + '/' + yyyy;
    return today;
}

function formatCurrency(num) {

    num = num.toString().replace(/\Rp|/g, '');
    if (isNaN(num))
        num = "0";
    sign = (num == (num = Math.abs(num)));
    num = Math.floor(num * 100 + 0.50000000001);
    cents = num % 100;
    num = Math.floor(num / 100).toString();
    if (cents < 10)
        cents = "0" + cents;
    for (var i = 0; i < Math.floor((num.length - (1 + i)) / 3) ; i++)
        num = num.substring(0, num.length - (4 * i + 3)) + '.' +
        num.substring(num.length - (4 * i + 3));
    //document.write(((sign) ? '' : '-') + 'Rp ' + num + ',' + cents);
    return (((sign) ? '' : '-') //+ 'Rp '
        + num //+ ',' + cents
        );
}