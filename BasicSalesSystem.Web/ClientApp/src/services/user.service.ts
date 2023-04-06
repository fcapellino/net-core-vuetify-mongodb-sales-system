import { AxiosStatic } from "axios";

export class UserService {
    private axios: AxiosStatic = (<any>window).axios;
    constructor() {
    }

    getUsersList(params: any) {
        var self = this;
        return self.axios
            .get('/api/users/getuserslist', {
                params
            })
    }

    getUsersRoleList() {
        var self = this;
        return self.axios
            .get('/api/users/getusersrolelist')
    }

    createUser(body: any) {
        var self = this;
        return self.axios
            .post('/api/users/createuser', body)
    }

    enableOrDisableUser(id: any) {
        var self = this;
        return self.axios
            .post('/api/users/enableordisableuser', {}, {
                params: { id }
            })
    }

    deleteUser(id: any) {
        var self = this;
        return self.axios
            .post('/api/users/deleteuser', {}, {
                params: { id }
            })
    }

    changePassword(body: any) {
        var self = this;
        return self.axios
            .post('/api/users/changepassword', body)
    }

    generateJSONWebToken(body: any) {
        var self = this;
        return self.axios
            .post('/api/users/generatejsonwebtoken', body)
    }

    refreshJSONWebToken(body: any) {
        var self = this;
        return self.axios
            .post('/api/users/refreshjsonwebtoken', body)
    }
}
