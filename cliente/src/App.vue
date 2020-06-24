<template>
    <div id="app">
        <div id="nav">
            <router-link to="/">Home</router-link> |
            <router-link to="/about" v-if="isLogged">About</router-link>
            <router-link to="/login" v-else>Login</router-link>
            <button type="button" @click="logout()" v-if="isLogged">
                Logout
            </button>
        </div>
        <div class="container">
            <div class="row">
                <router-view/>
            </div>
        </div>
    </div>
</template>

<script>
    import { mapGetters } from 'vuex'

    export default {
        computed: {
            ...mapGetters([
                'isLogged'
            ])
        },

        methods: {
            logout () {
                this.$store.dispatch('logout')
            }
        }
    }
</script>