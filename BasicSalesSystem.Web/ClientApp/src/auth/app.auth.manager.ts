import jwtDecode from 'jwt-decode';
import { UserService } from '../services/user.service';

export class AuthManager {
    private userService: UserService;
    private roles: string[] = [];

    constructor(userService: UserService) {
        this.userService = userService;
        this.readClaimsFromLocalStorage();
    }

    public async login(credentials) {
        var self = this;
        try {
            var response = await self.userService.generateJSONWebToken(credentials);
            var error = response?.data?.error;

            if (error === false) {
                var resources = response.data.resources;
                var jwtToken = resources.token;
                localStorage.setItem('token', jwtToken);
            }
        }
        catch (e) { }
        finally {
            self.readClaimsFromLocalStorage();
        }
    }

    public logout() {
        var self = this;
        localStorage.removeItem('token');
        self.roles = [];
    }

    public readClaimsFromLocalStorage() {
        var self = this;
        var token = localStorage.getItem('token');

        if (token) {
            var decodedToken = jwtDecode(token);
            self.roles = Array.isArray(decodedToken.role)
                ? decodedToken.role
                : new Array(decodedToken.role);

            var claims = Object.assign({}, decodedToken);
            return claims;
        }
    }

    public authenticated() {
        var self = this;
        var token = localStorage.getItem('token');

        if (!token) {
            return false;
        }

        var decodedToken = jwtDecode(token);
        var expirationDate = new Date(0);
        expirationDate.setUTCSeconds(decodedToken.exp);
        return expirationDate > new Date();
    }

    public isInRole(roleName) {
        var self = this;
        return self.roles.indexOf(roleName) > -1;
    }

    public async tryRefreshToken() {
        var self = this;
        try {
            var token = localStorage.getItem('token');
            if (token) {
                var bodyData = { token: localStorage.getItem('token') };
                var response = await self.userService.refreshJSONWebToken(bodyData);
                var error = response?.data?.error;

                if (error === false) {
                    var resources = response.data.resources;
                    var jwtToken = resources.token;
                    localStorage.setItem('token', jwtToken);
                }
            }
        }
        catch (e) { }
        finally {
            self.readClaimsFromLocalStorage();
        }
    }
}
