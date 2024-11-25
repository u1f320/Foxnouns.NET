<script lang="ts">
	import { Pagination, PaginationItem, PaginationLink } from "@sveltestrap/sveltestrap";

	type Props = { currentPage: number; pageCount: number; href: string; center?: boolean };
	let { currentPage, pageCount, href, center }: Props = $props();

	let prevPage = $derived(currentPage > 0 ? currentPage - 1 : 0);
	let prevLink = $derived(prevPage !== 0 ? `${href}?page=${prevPage}` : href);

	let nextPage = $derived(currentPage < pageCount - 1 ? currentPage + 1 : pageCount - 1);
</script>

{#if pageCount > 1}
	<div>
		<Pagination listClassName={center ? "justify-content-center" : undefined}>
			<PaginationItem>
				<PaginationLink first {href} />
			</PaginationItem>
			<PaginationItem>
				<PaginationLink previous href={prevLink} />
			</PaginationItem>
			{#each new Array(pageCount) as _, page}
				<PaginationItem active={page === currentPage}>
					<PaginationLink href="{href}?page={page}">{page + 1}</PaginationLink>
				</PaginationItem>
			{/each}
			<PaginationItem>
				<PaginationLink next href="{href}?page={nextPage}" />
			</PaginationItem>
			<PaginationItem>
				<PaginationLink last href="{href}?page={pageCount - 1}" />
			</PaginationItem>
		</Pagination>
	</div>
{/if}
