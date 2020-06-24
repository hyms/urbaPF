<template>
    <div class="account-container">
        <div class="content clearfix">
                <form @submit.prevent="login">
                <h1>Ingresar</h1>

                <div>
                    <div v-if="m_error">
                        <b-alert show dismissible variant="danger" v-for="(value, key) in m_error" :key="key">
                            {{ key }}: {{ value }}
                        </b-alert>
                    </div>
                    <p>Por favor Ingrese sus datos</p>
                    <b-form-group>
                        <b-form-input v-model="email" placeholder="Usuario"></b-form-input>
                    </b-form-group>
                    <b-form-group>
                        <b-form-input type="password" v-model="password" placeholder="Contraseña"></b-form-input>
                    </b-form-group>

                </div> <!-- /login-fields -->

                <div class="login-actions">

				<span class="login-checkbox">
					<input id="Field" name="Field" type="checkbox" class="field login-checkbox" value="First Choice"
                           tabindex="4"/>
					<label class="choice" for="Field">Keep me signed in</label>
				</span>

                    <b-button type="submit" variant="primary btn-large button">Ingresar</b-button>

                </div> <!-- .actions -->

                </form>
        </div> <!-- /content -->
    </div> <!-- /account-container -->

</template>

<script>
    require('@/assets/css/pages/signin.css');

    export default {
        data() {
            return {
                email: '',
                password: '',
                m_error: false,
            }
        },

        methods: {
            async login() {
                await this.$store
                    .dispatch('login', {
                        email: this.email,
                        password: this.password
                    })
                    .then(() => {
                        this.$router.push({name: 'About'})
                    })
                    .catch(err => {
                        console.log(err);
                    });

                try {
                    this.m_error = JSON.parse(this.$store.state.message);
                } catch (e) {
                    this.m_error = {"mensaje":[this.$store.state.message]};
                }

            }
        }
    }
</script>
