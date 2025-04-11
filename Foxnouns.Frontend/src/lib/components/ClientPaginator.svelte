<script lang="ts">
	import { Pagination, PaginationItem, PaginationLink } from "@sveltestrap/sveltestrap";

	type Props = {
		currentPage: number;
		pageCount: number;
		center?: boolean;
		listAllPages?: boolean;
	};
	let { currentPage = $bindable(), pageCount, center, listAllPages }: Props = $props();

	let allPages = $derived(listAllPages === undefined || listAllPages);

	let prevPage = $derived(currentPage > 0 ? currentPage - 1 : 0);
	let nextPage = $derived(currentPage < pageCount - 1 ? currentPage + 1 : pageCount - 1);
</script>

{#if pageCount > 1}
	<div>
		<Pagination listClassName={center ? "justify-content-center" : undefined}>
			<PaginationItem>
				<PaginationLink first onclick={() => (currentPage = 0)} />
			</PaginationItem>
			<PaginationItem>
				<PaginationLink previous onclick={() => (currentPage = prevPage)} />
			</PaginationItem>
			{#if allPages}
				{#each new Array(pageCount) as _, page}
					<PaginationItem active={page === currentPage}>
						<PaginationLink onclick={() => (currentPage = page)}>{page + 1}</PaginationLink>
					</PaginationItem>
				{/each}
			{:else}
				{#if currentPage !== 0}
					<PaginationItem onclick={() => (currentPage = prevPage)}>
						<PaginationLink>{currentPage}</PaginationLink>
					</PaginationItem>
				{/if}
				<PaginationItem active>
					<PaginationLink>{currentPage + 1}</PaginationLink>
				</PaginationItem>
				{#if currentPage !== pageCount - 1}
					<PaginationItem onclick={() => (currentPage = nextPage)}>
						<PaginationLink>{currentPage + 2}</PaginationLink>
					</PaginationItem>
				{/if}
			{/if}
			<PaginationItem>
				<PaginationLink next onclick={() => (currentPage = nextPage)} />
			</PaginationItem>
			<PaginationItem>
				<PaginationLink last onclick={() => (currentPage = pageCount - 1)} />
			</PaginationItem>
		</Pagination>
	</div>
{/if}
