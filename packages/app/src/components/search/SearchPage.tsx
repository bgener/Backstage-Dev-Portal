import React from 'react';
import { Content, Header, Page } from '@backstage/core-components';
import { Grid, List, Paper } from '@material-ui/core';
import {
  CatalogSearchResultListItem,
  catalogApiRef,
} from '@backstage/plugin-catalog';
import {
  SearchType,
  DefaultResultListItem,
  SearchBar,
  SearchFilter,
  SearchResult,
  SearchPagination,
  useSearch,
} from '@backstage/plugin-search-react';
import { TechDocsSearchResultListItem } from '@backstage/plugin-techdocs';
import { useApi } from '@backstage/core-plugin-api';

const SearchPage = () => {
  const { types } = useSearch();
  const catalogApi = useApi(catalogApiRef);

  return (
    <Page themeId="home">
      <Header title="Search" />
      <Content>
        <Grid container direction="row">
          <Grid item xs={12}>
            <SearchBar />
          </Grid>
          <Grid item xs={3}>
            <SearchType.Accordion
              name="Result Type"
              defaultValue="software-catalog"
              types={[
                {
                  value: 'software-catalog',
                  name: 'Software Catalog',
                  icon: <CatalogSearchResultListItem.icon />,
                },
                {
                  value: 'techdocs',
                  name: 'Documentation',
                  icon: <TechDocsSearchResultListItem.icon />,
                },
              ]}
            />
            <Paper>
              <SearchFilter.Select
                label="Kind"
                name="kind"
                values={['Component', 'Template']}
              />
              <SearchFilter.Checkbox
                label="Lifecycle"
                name="lifecycle"
                values={['experimental', 'production']}
              />
            </Paper>
          </Grid>
          <Grid item xs={9}>
            <SearchPagination />
            <SearchResult>
              <CatalogSearchResultListItem icon={<CatalogSearchResultListItem.icon />} />
              <TechDocsSearchResultListItem icon={<TechDocsSearchResultListItem.icon />} />
              <DefaultResultListItem />
            </SearchResult>
          </Grid>
        </Grid>
      </Content>
    </Page>
  );
};

export const searchPage = <SearchPage />;
