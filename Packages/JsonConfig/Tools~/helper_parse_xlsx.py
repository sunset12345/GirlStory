#!/usr/bin/env python
# -*- coding: utf-8 -*-

import openpyxl
from cls_workbook import WorkBook, WorkSheet
import config

def Parse(file):
    workbook = openpyxl.load_workbook(file)
    book = WorkBook()
    
    for i in range(len(workbook.worksheets)):
        worksheet = workbook.worksheets[i]
        sTitle = worksheet.title
        if i>0 and 'merge#' not in sTitle and 'out#' not in sTitle and 'group#' not in sTitle:
            continue
        
        ayrows = list(worksheet.rows)
        aycols = list(worksheet.columns)
        nrows = len(ayrows)  # 行数
        ncols = len(aycols)  # 列数
        sheet = WorkSheet(nrows, ncols)
        sheet.sTitle = sTitle
        sheet.startRow = config.start_row_num
        
        for row in range(1, nrows + 1):
            for column in range(1, ncols + 1):
                sheet.Cell(row, column, worksheet.cell(row, column).value)
        # sheet.ToString()
        if i>0 and 'merge#' in sTitle:
            book.sheets[0].Merge(sheet)
        else:
            book.sheets.append(sheet)
    # book.sheets[0].ToString()
    return book