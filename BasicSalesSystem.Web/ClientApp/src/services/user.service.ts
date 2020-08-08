import axios from "axios";

export class UserService {
    constructor() {
    }

    getUsersList(params: any) {
        return axios
            .get('/api/users/getuserslist', {
                params
            })
    }

    getUsersRoleList() {
        return axios
            .get('/api/users/getusersrolelist')
    }

    createUser(body: any) {
        return axios
            .post('/api/users/createuser', body)
    }

    deleteUser(id: any) {
        return axios
            .post('/api/users/deleteuser', {}, {
                params: { id }
            })
    }

    changePassword(body: any) {
        return axios
            .post('/api/users/changepassword', body)
    }

    generateJSONWebToken(body: any) {
        return axios
            .post('/api/users/generatejsonwebtoken', body)
    }
}
