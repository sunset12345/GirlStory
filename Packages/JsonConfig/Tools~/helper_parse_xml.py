#!/usr/bin/env python
# -*- coding: utf-8 -*-

import xml.dom.minidom
from cls_workbook import WorkBook, WorkSheet
import config

def Parse(file):
    xmls = xml.dom.minidom.parse(file)
    document = xmls.documentElement
    book = WorkBook()

    worksheets = document.getElementsByTagName('Worksheet')
    for i in range(len(worksheets)):
        worksheet = worksheets[i]
        sTitle = worksheet.getAttribute('ss:Name')
        if i>0 and 'merge#' not in sTitle and 'out#' not in sTitle and 'group#' not in sTitle:
            continue
        table = worksheet.getElementsByTagName('Table')[0]
        ayrows = table.getElementsByTagName('Row') # 行数
        nrows = 0
        ncols = 0
        for row_cell in ayrows:
            nrows = _parse_xml_get_index(row_cell, nrows) 
            ncols = max(ncols, len(row_cell.getElementsByTagName('Cell'))) # 列数
        
        if nrows == 0: 
            continue
        
        sheet = WorkSheet(nrows, ncols)
        sheet.sTitle = sTitle
        sheet.startRow = config.start_row_num
        # print("========== title", sheet.sTitle)
        
        for row in range(0, nrows):
            for column in range(0, ncols):
                sheet.Cell(row + 1, column + 1, None)
        
        row = 0
        for row_cell in ayrows:
            row = _parse_xml_get_index(row_cell, row)  
            ayCells = row_cell.getElementsByTagName('Cell')
            column = 0
            # s = ''
            for cell in ayCells:
                column = _parse_xml_get_index(cell, column)
                datas = _parse_xml_get_data(cell)
                value = _parse_xml_get_value(datas)
                if len(value) == 0:
                    value = None
                # s += '#%s ' % (value)
                sheet.Cell(row, column, value)
            # print(">>>>>row", row, s)
            
        # sheet.ToString() 
        if i>0 and 'merge#' in sTitle:
            book.sheets[0].Merge(sheet)
        else:
            book.sheets.append(sheet)
    
    # 格式化xml
    # strXml = xmls.toprettyxml(encoding = "utf-8")
    # strXml = os.linesep.join([s for s in strXml.splitlines() if s.strip()])
    # with open(file, 'w') as f:
    #     f.write(strXml)
    
    return book

def _parse_xml_get_index(cell, value):
    offIndex = cell.getAttribute('ss:Index')
    if offIndex is '':
        value += 1
    else:
        value = int(offIndex)
    return value
    
def _parse_xml_get_data(cell):
    datas = None
    for child in cell.childNodes:
        if child.localName == u'Data':
            datas = child
            break
    return datas
        
def _parse_xml_get_value(datas):
    value = ''
    if datas is None:
        return value
    for child in datas.childNodes:
        if child.localName == u'Font':
            value += _parse_xml_get_value(child)
        if child.nodeValue is None:
            continue
        s = child.nodeValue.strip()
        value += s
    return value.strip()