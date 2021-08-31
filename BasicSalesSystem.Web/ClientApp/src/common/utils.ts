export abstract class Utils {
    public static formatDate(date: any): string {
        if (!date) return null!;
        var e = new Date(date).toISOString().slice(0, -1);
        const [n, r, i] = e.slice(0, 10).split("-");
        return `${i}/${r}/${n}`;
    }
    public static isNullOrEmpty(text: any): boolean {
        return (text) ? (text.toString().match(/^ *$/) !== null) : true
    }
    public static isValidEmail(text: any): boolean {
        return /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(String(text).toLowerCase())
    }
    public static parseDate(text: any): Date {
        if (!text) return null!;
        var e = text.split("/");
        return new Date(e[2], e[1] - 1, e[0]);
    }
    public static tryGet(func: any): any {
        try {
            var value = func()
            if (value) {
                return value
            }
            else {
                return undefined
            }
        } catch (e) {
            return undefined
        }
    }
    public static tryGetNumber(func: any): any {
        try {
            var value = func();
            return value && !isNaN(value) ? value : 0;
        } catch (t) {
            return 0;
        }
    }
}
