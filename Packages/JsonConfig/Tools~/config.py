#!/usr/bin/env python
# -*- coding: utf-8 -*-
###################################

global json_path
global cls_path
global sheet_path
global replenish_path_start

global suffix_xlsx
global suffix_xml
global suffixs
global filters
global ignore_cpp
global ignore_load
global start_row_num
global cpp_end_name
global class_description

# config.json_path = "/../Assets/_ConfigBase/Resources/"
# config.cls_path = "/../Assets/_ConfigBase/Script/"
json_path = None #json输出位置
cls_path = None #script输出位置
sheet_path = None #表所在位置
replenish_path_start = None #文件输出时 再Resources路径下级时 自动在头补充缺失路径  Resources/Config/ => Config/

suffix_xlsx = 'xlsx'
suffix_xml = 'xml'
suffix_csv = 'csv'
suffixs = (suffix_xlsx, suffix_xml, suffix_csv)
filters = '~' #忽略文件
ignore_cpp = "#" #生成json 不生成cs 不自动加载
ignore_load = "@" #生成json 生成cs 不自动加载 
start_row_num = 3
cpp_end_name = ''
class_description = 'ClassDescription'
inheritance_description = 'InheritanceDescription'
