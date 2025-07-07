#!/usr/bin/env python
# -*- coding: utf-8 -*-

import csv
from cls_workbook import WorkBook, WorkSheet

def Parse(file):
    with open(file, mode='r') as f:
        reader = csv.reader(f)
        worksheet = []
        for row in reader:
            worksheet.append(list(row))

    # print(worksheet)

    book = WorkBook()
    sTitle = 'Sheet'

    if len(worksheet) == 0:
        return book
    
    nrows = len(worksheet)  # 行数
    ncols = len(worksheet[0])  # 列数
    sheet = WorkSheet(nrows, ncols)
    sheet.sTitle = sTitle
    sheet.startRow = 3
    
    for row in range(0, nrows):
        for column in range(0, ncols):
            sheet.Cell(row + 1, column + 1, worksheet[row][column])
    book.sheets.append(sheet)

    # sheet.ToString()
    return book