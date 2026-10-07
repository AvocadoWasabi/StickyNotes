[English](../docs/TASKS-CLI-PREVIEW.md#english) | [日本語](../docs/TASKS-CLI-PREVIEW.md#日本語) | [简体中文](../docs/TASKS-CLI-PREVIEW.md#简体中文)

# Tasks CLI Preview

Copy this file into your test vault and open it in the preview app. The queries below search the whole vault, limited to 5 results each; existing global Tasks settings still apply. The guide links above refer to the extracted package.

このファイルをテスト用Vaultへコピーし、試験版で開いてください。下のクエリはVault全体を検索し、それぞれ最大5件表示します。既存のTasksのグローバル設定も適用されます。上のガイドリンクは展開した配布フォルダ内の資料を指します。

请将此文件复制到测试仓库并在试验版打开。下面的查询搜索整个仓库，每个最多显示5项；已有的 Tasks 全局设置仍然适用。上方指南链接指向解压后的分发文件夹。

## Ordinary Markdown / 通常のMarkdown / 普通 Markdown

**Text before the query / クエリ前の文章 / 查询前的文字**

- [ ] Local checkbox / 通常のチェックボックス / 普通复选框

## Open tasks / 未完了 / 未完成

```tasks
not done
sort by due
group by filename
limit 5
```

This paragraph stays between the two queries. / この文章は2つのクエリの間に残ります。 / 这段文字保留在两个查询之间。

## Done tasks / 完了 / 已完成

```tasks
done
sort by done reverse
limit 5
```

Text after the queries. / クエリ後の文章。 / 查询后的文字。
