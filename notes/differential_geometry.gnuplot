set parametric
set trange [0:0.5 * pi]
set yrange [-3:+3]
set xrange [-3:+3]
set samples 20
set terminal wxt size 800, 800
f0x(t) = sin(2 * t)
f0y(t) = 2 * cos(t)
f1x(t) = 2 * cos(2 * t)
f1y(t) = - 2 * sin(t)
f2x(t) = - 4 * sin(2 * t)
f2y(t) = - 2 * cos(t)
plot f0x(t), f0y(t) with points, f1x(t), f1y(t) with points, f2x(t), f2y(t) with points
