//import { Injectable } from '@angular/core';
//import { ActivatedRouteSnapshot, CanActivate, Router } from '@angular/router';
//import * as _ from 'lodash';
//import { Notify } from '../app/shared/common/notify';
//import { AuthManager } from './app.auth.manager';

//@Injectable()
//export class AuthGuard implements CanActivate {

//    constructor(
//        private _router: Router,
//        private _authManager: AuthManager) {
//    }

//    canActivate(snapshot: ActivatedRouteSnapshot) {
//        var self = this

//        var roles = snapshot.data["roles"] as Array<string>
//        if (self._authManager.authenticated()) {

//            var authorized = roles?.every(r => self._authManager.isInRole(r))
//            return roles ? authorized : true
//        }
//        else {
//            self._router.navigate(['/login'])
//            self.notify('Error. Invalid or expired token. Please re-login.')
//            return false
//        }
//    }

//    notify = _.debounce((errorMessage: string) => {
//        Notify.showErrorNotification(errorMessage)
//    }, 100)
//}
