<template>
    <div class="col-sm-8 col-md-6 col align-self-center">
        <b-card title="Login">
        <b-form @submit.prevent="login" @reset="onReset">
            <b-form-group
                    id="input-group-1"
                    label="Usuario:"
                    label-for="input-1"
            >
                <b-form-input
                        id="input-1"
                        v-model="email"
                        type="text"
                        required
                        placeholder="Usuario"
                ></b-form-input>
            </b-form-group>
            <b-form-group
                    id="input-group-2"
                    label="Contraseña:"
                    label-for="input-2"
            >
                <b-form-input
                        id="input-2"
                        v-model="password"
                        type="password"
                        required
                        placeholder="Contraseña"
                ></b-form-input>
            </b-form-group>
            <b-button type="submit" variant="primary">Submit</b-button>
            <b-button type="reset" variant="danger">Reset</b-button>
            <div v-if="m_error">
            <b-alert show dismissible variant="danger" v-for="(value, key) in m_error" :key="key">
                {{ key }}: {{ value }}
            </b-alert>
            </div>
        </b-form>
        </b-card>
    </div>
</template>

<script>
    export default {
        data () {
            return {
                email: '',
                password: '',
                m_error: false,
            }
        },

        methods: {
            async login () {
                 await this.$store
                    .dispatch('login', {
                        email: this.email,
                        password: this.password
                    })
                    .then(() => {
                        this.$router.push({ name: 'About' })
                    })
                    .catch(err => {
                        console.log(err);
                    });

                this.m_error = JSON.parse(JSON.stringify(this.$store.state.message))
            }
        }
    }
</script>
