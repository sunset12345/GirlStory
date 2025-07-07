#!/usr/bin/env python
# -*- coding: utf-8 -*-

import config

def cells_is_null(cells):
    for cell in cells:
        if cell.value is not None:
            return False
    return True

class WorkBook:
    sheets = None
    def __init__(self):
        self.sheets = []
        pass

class WorkCell:
    value = None
    def __init__(self):
        self.value = None
        pass

class WorkSheet:
    sTitle = ''
    nRowLen = 0
    nColLen = 0
    dyCells = None
    startRow = 3
    
    def __init__(self, row_len, col_len):
        self.nRowLen = row_len
        self.nColLen = col_len
        self.dyCells = {}
    
    @property
    def Rows(self):
        for row in range(1, self.nRowLen + 1):
            cells = (self.Cell(row, column) for column in range(1, self.nColLen + 1))
            yield tuple(cells)
    
    @property
    def Columns(self):
        for column in range(1, self.nColLen + 1):
            cells = (self.Cell(row, column) for row in range(1, self.nRowLen + 1))
            yield tuple(cells)
    
    def Cell(self, row, column, value=None):
        if row < 1 or column < 1:
            raise ValueError("Row or column values must be at least 1")

        cell = self._get_cell(row, column)
        if value is not None:
            cell.value = value
        return cell
    
    def _get_cell(self, row, column):
        coordinate = (row, column)
        if not coordinate in self.dyCells:
            self._add_cell(row, column)
        return self.dyCells[coordinate]
    
    def _add_cell(self, row, column):
        cell = WorkCell()
        cell.value = None
        self.dyCells[(row, column)] = cell
        
    def Merge(self, workcell):
        rowIndex = self.nRowLen
        self.nRowLen += workcell.nRowLen - self.startRow
        for k,v in workcell.dyCells.items():
            index = k[0] - self.startRow
            if index < 1:
                continue
            self.dyCells[(rowIndex+index, k[1])] = v
        
        
    def ToString(self):
        print('WorkSheet== %s ==row:%s col:%s' % (self.sTitle, self.nRowLen, self.nColLen) )
        for row in range(1, self.nRowLen + 1):
            s = ''
            for column in range(1, self.nColLen + 1):
                s += str(self.Cell(row, column).value) + ' '
            print(u'>>row:%s  :%s' % (row, s))