set parametric
set trange [0:pi]
set yrange [-3:+3]
set xrange [-3:+3]
plot sin(2 * t), 2 * cos(t), cos(2 * t) * 2, - 2 * sin(t), -sin(2 * t) * 4, - 2 * cos(t)
