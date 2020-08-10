export abstract class Utils {
    public static isNullOrEmpty(text: any): boolean {
        return (text) ? (text.toString().match(/^ *$/) !== null) : true
    }
    public static isValidEmail(text: any): boolean {
        return /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/.test(String(text).toLowerCase())
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
