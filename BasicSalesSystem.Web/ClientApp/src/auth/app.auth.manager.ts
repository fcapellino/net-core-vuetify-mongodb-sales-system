//import { Injectable } from '@angular/core';
//import { JwtHelper } from 'angular2-jwt';
//import { UserService } from '../services/user.service';

//@Injectable()
//export class AuthManager {
//    private roles: string[] = []
//    private jwtHelper: JwtHelper = new JwtHelper()

//    constructor(private _userService: UserService) {
//        this.readClaimsFromLocalStorage()
//    }

//    public async login(credentials) {
//        try {
//            var self = this
//            var response = await self._userService.generateJSONWebToken(credentials).toPromise<any>()
//            if (response?.error === false) {
//                var jwtToken = response.resources.token
//                localStorage.setItem('token', jwtToken)
//            }
//        }
//        catch (e) { }
//        finally {
//            self.readClaimsFromLocalStorage()
//        }
//    }

//    public logout() {
//        var self = this
//        localStorage.removeItem('token')
//        self.roles = []
//    }

//    public readClaimsFromLocalStorage() {
//        var self = this

//        var token = localStorage.getItem('token')
//        if (token) {
//            var decodedToken = self.jwtHelper.decodeToken(token)
//            self.roles = Array.isArray(decodedToken.role)
//                ? decodedToken.role
//                : new Array(decodedToken.role)
//        }
//    }

//    public authenticated() {
//        var self = this
//        var token = localStorage.getItem('token')
//        return token ? !self.jwtHelper.isTokenExpired(token) : false
//    }

//    public isInRole(roleName) {
//        var self = this
//        return self.roles.indexOf(roleName) > -1
//    }
//}
