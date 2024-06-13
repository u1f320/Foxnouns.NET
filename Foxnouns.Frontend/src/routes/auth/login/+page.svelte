<script lang="ts">
	import type { PageData } from "./$types";
	export let data: PageData;
	$: hasUrls = !!(data.urls.discord || data.urls.google || data.urls.tumblr);
</script>

<div class="container mt-6">
	<div class="fixed-grid has-1-cols has-2-cols-desktop">
		<div class="grid">
			<div class="cell">
				<p class="title">Log in with email address</p>
				<form method="POST" action="?/login">
					<div class="field">
						<label for="email" class="label">Email address</label>
						<div class="control">
							<input type="email" id="email" class="input" placeholder="Email address" />
						</div>
					</div>
					<div class="field">
						<label for="password" class="label">Password</label>
						<div class="control">
							<input type="password" id="password" class="input" placeholder="Password" />
						</div>
					</div>
					<div class="field is-grouped">
						<div class="control">
							<button class="button is-primary">Log in</button>
						</div>
						<div class="control">
							<a href="/auth/signup" class="button">Sign up</a>
						</div>
					</div>
				</form>
			</div>
			{#if hasUrls}
				<div class="cell">
					<p class="title">Log in with third-party provider</p>
					<ul>
						{#if data.urls.discord}
							<li><a href={data.urls.discord}>Log in with Discord</a></li>
						{/if}
                        {#if data.urls.google}
							<li><a href={data.urls.google}>Log in with Google</a></li>
						{/if}
                        {#if data.urls.tumblr}
							<li><a href={data.urls.tumblr}>Log in with Tumblr</a></li>
						{/if}
					</ul>
				</div>
			{/if}
		</div>
	</div>
</div>
