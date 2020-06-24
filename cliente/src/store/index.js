import Vue from 'vue'
import Vuex from 'vuex'
import axios from 'axios'

Vue.use(Vuex);

axios.defaults.withCredentials = true;
axios.defaults.baseURL = 'http://localhost/EquipoHRM/api/public/';
axios.defaults.headers.post['Content-Type'] = 'application/x-www-form-urlencoded';


export default new Vuex.Store({
    state: {
        user: null,
        message: null,
    },

    mutations: {
        setUserData (state, userData) {
            state.user = userData;
            localStorage.setItem('user', JSON.stringify(userData));
            axios.defaults.headers.common.Authorization = `Bearer ${userData.token}`
        },

        clearUserData () {
            localStorage.removeItem('user');
            location.reload()
        }
    },

    actions: {
        async login ({ commit }, credentials) {
            await axios.get('/sanctum/csrf-cookie')
            return axios
                .post('api/login', credentials)
                .then(({ data }) => {
                    if(data['status_code']===200){
                        commit('setUserData', data)
                    }
                    else {
                        this.state.message = data['message'];
                    }
                })
        },

        async logout ({ commit }) {
            //await axios.post('api/logout');
            commit('clearUserData')
        }
    },

    getters : {
        isLogged: state => !!state.user
    }
})
